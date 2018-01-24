using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Net;

namespace tellick_admin.Cli {
    [DataContract]  
    public class Customer {
        [DataMember]  
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]  
    public class Project {
        [DataMember]  
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int CustomerId { get; set; }
    }

    [DataContract]  
    public class Log {
        [DataMember]  
        public int Id { get; set; }
        [DataMember]  
        public float Hours { get; set; }
        [DataMember]  
        public string Message { get; set; }
        [DataMember]  
        public int ProjectId { get; set; }
    }

    public class Cli {
        private readonly HttpClient _client;
        private readonly TpConfig _tpConfig;

        public Cli(TpConfig tpConfig) {
            _client  = new HttpClient();
            _tpConfig = tpConfig;
        }

        public async Task ParseAndRun(string[] args) {
            switch (args[0])
            {
                case "new":
                    if (args.Length < 2) {
                        Console.WriteLine("Please provide an entity type to create, such as [customer], [project] or [...]");
                    } else {
                        switch (args[1])
                        {
                            case "customer":
                                await CreateCustomer(args);
                                break;
                            case "project":
                                await CreateProject(args);
                                break;
                            default:
                                Console.WriteLine("Unknown entity type '{0}'", args[1]);
                                break;
                        }
                    }
                    break;
                case "active":
                    if (args.Length < 2) {
                        Console.Write("Please provide a project name.");
                    } else {
                        await SetActive(args);
                    }
                    break;
                case "log":
                    if (args.Length < 3) {
                        Console.Write("Please provide an amount of hours and a message like this: tp log [hours] [message]");
                    } else {
                        await Log(args);
                    }
                    break;
                case "customers":
                    await ShowCustomers();
                    break;
                case "projects":
                    await ShowProjects();
                    break;
                default:
                    Console.WriteLine("Unknown command");
                    break;
            }
        }

        public async Task CreateCustomer(string[] args) {
            if (args.Length == 2) {
                Console.WriteLine("Provide a customer name.");
                return;
            }

            if (args.Length > 3) {
                Console.WriteLine("Too many argumnts. Did you mean to use double quotes?");
                return;
            }

            if (args[2].Length == 0) {
                Console.WriteLine("Customer name cannot be empty");
                return;
            }

            Customer c = new Customer();
            c.Name = args[2];
            string jsonContent = JsonConvert.SerializeObject(c);
            await _client.PostAsync(_tpConfig.Origin + "/api/customer", new StringContent(jsonContent, Encoding.UTF8, "application/json"));

            Console.WriteLine("Customer '{0}' created.", args[2]);
        }

        public async Task CreateProject(string[] args) {
            if (args.Length != 4) {
                Console.WriteLine("Invalid amount of arguments use 'tp new project [projectname] [customername]'.");
                return;
            }

            string projectName = args[2];
            string customerName = args[3];
            
            HttpResponseMessage message = await _client.GetAsync(_tpConfig.Origin + "/api/customer/" + WebUtility.UrlEncode(customerName));
            string messageContent = await message.Content.ReadAsStringAsync();
            Customer customer = JsonConvert.DeserializeObject<Customer>(messageContent);
            if (customer != null) {
                Project p = new Project();
                p.Name = projectName;
                p.CustomerId = customer.Id;
                string jsonContent = JsonConvert.SerializeObject(p);
                await _client.PostAsync("http://localhost:5000/api/project", new StringContent(jsonContent, Encoding.UTF8, "application/json"));
                Console.WriteLine("Project '{0}' created and connected to customer {1}.", projectName, customerName);
            } else {
                Console.WriteLine("Unknown customer '{0}'", customerName);
            }
        }

        public async Task SetActive(string[] args) {
            string projectName = args[1];
            HttpResponseMessage message = await _client.GetAsync(_tpConfig.Origin + "/api/project/" + WebUtility.UrlEncode(projectName));
            if (message.StatusCode == HttpStatusCode.NotFound) {
                Console.WriteLine("Cannot activate project '{0}' as it does not exist!", projectName);
            } else {
                _tpConfig.ActiveProject = projectName;
                TpConfigReaderWriter tpConfigReaderWriter = new TpConfigReaderWriter();
                tpConfigReaderWriter.TpConfig = _tpConfig;
                await tpConfigReaderWriter.WriteConfig();
                Console.WriteLine("Active project set to '{0}'", projectName);
            }
        }

        public async Task Log(string[] args) {
            HttpResponseMessage message = await _client.GetAsync(_tpConfig.Origin + "/api/project/" + WebUtility.UrlEncode(_tpConfig.ActiveProject));
            if (message.StatusCode == HttpStatusCode.NotFound) {
                Console.WriteLine("Cannot log to project '{0}' as it does not exist. Activate a different project.", _tpConfig.ActiveProject);
            } else {
                string messageContent = await message.Content.ReadAsStringAsync();
                Project p = JsonConvert.DeserializeObject<Project>(messageContent);

                Log l = new Log();
                float hours;
                if (Single.TryParse(args[1], out hours) == false) {
                    Console.WriteLine("Incorrect hours format.");
                    return;
                }
                l.Hours = hours;
                l.Message = args[2];
                l.ProjectId = p.Id;

                string jsonContent = JsonConvert.SerializeObject(l);
                await _client.PostAsync("http://localhost:5000/api/log", new StringContent(jsonContent, Encoding.UTF8, "application/json"));
                Console.WriteLine("Logged {0} hours to project {1}.", hours, p.Name);
            }
        }

        public async Task ShowCustomers() {
            HttpResponseMessage message = await _client.GetAsync(_tpConfig.Origin + "/api/customer");
            string messageContent = await message.Content.ReadAsStringAsync();
            Customer[] customers = JsonConvert.DeserializeObject<Customer[]>(messageContent);
            foreach (var item in customers) {
                Console.WriteLine("{0, -5} {1}", item.Id, item.Name);
            }
        }

        public async Task ShowProjects() {
            HttpResponseMessage message = await _client.GetAsync(_tpConfig.Origin + "/api/project");
            string messageContent = await message.Content.ReadAsStringAsync();
            Console.WriteLine(messageContent);
            Project[] projects = JsonConvert.DeserializeObject<Project[]>(messageContent);
            foreach (var item in projects) {
                Console.WriteLine("{0, -5} {1, -20} {2}", item.Id, item.Name, item.CustomerId);
            }
        }
    }
}