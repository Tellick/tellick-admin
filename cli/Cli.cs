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
using System.Globalization;
using System.Linq;

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
        [DataMember]
        public Customer Customer { get; set; }
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
        public DateTime ForDate { get; set; }
        [DataMember]  
        public int ProjectId { get; set; }
        [DataMember]  
        public Project Project { get; set; }
    }

    public class Cli {
        private readonly HttpClient _client;
        private readonly TpConfig _tpConfig;

        public Cli(TpConfig tpConfig) {
            _tpConfig = tpConfig;
            _client  = new HttpClient();
            _client.DefaultRequestHeaders.Add("Authorization", new string[] { "Bearer " + _tpConfig.Bearer });
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
                case "show":
                    await Show(args);
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
            HttpResponseMessage message =  await _client.PostAsync(_tpConfig.Origin + "/api/customer", new StringContent(jsonContent, Encoding.UTF8, "application/json"));

            if (message.StatusCode == HttpStatusCode.OK) {
                Console.WriteLine("Customer '{0}' created.", args[2]);
            } else if (message.StatusCode == HttpStatusCode.BadRequest) {
                string messageContent = await message.Content.ReadAsStringAsync();
                Console.WriteLine("Failed: ", messageContent);
            } else {
                Console.WriteLine("Error: {0}", message.StatusCode.ToString());
            }
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
                HttpResponseMessage response = await _client.PostAsync(_tpConfig.Origin + "/api/project", new StringContent(jsonContent, Encoding.UTF8, "application/json"));
                if (response.StatusCode == HttpStatusCode.OK) {
                    Console.WriteLine("Project '{0}' created and connected to customer {1}.", projectName, customerName);
                } else {
                    Console.WriteLine("Error: {0}", message.StatusCode.ToString());
                }
            } else {
                Console.WriteLine("Unknown customer '{0}'", customerName);
            }
        }

        public async Task SetActive(string[] args) {
            string projectName = args[1];
            HttpResponseMessage message = await _client.GetAsync(_tpConfig.Origin + "/api/project/" + WebUtility.UrlEncode(projectName));
            if (message.StatusCode == HttpStatusCode.NotFound) {
                Console.WriteLine("Cannot activate project '{0}' as it does not exist!", projectName);
            } else if (message.StatusCode == HttpStatusCode.OK) {
                _tpConfig.ActiveProject = projectName;
                TpConfigReaderWriter tpConfigReaderWriter = new TpConfigReaderWriter();
                tpConfigReaderWriter.TpConfig = _tpConfig;
                await tpConfigReaderWriter.WriteConfig();
                Console.WriteLine("Active project set to '{0}'", projectName);
            } else {
                Console.WriteLine("Error: {0}", message.StatusCode.ToString());
            }
        }

        public async Task Log(string[] args) {
            HttpResponseMessage message = await _client.GetAsync(_tpConfig.Origin + "/api/project/" + WebUtility.UrlEncode(_tpConfig.ActiveProject));
            if (message.StatusCode == HttpStatusCode.BadRequest) {
                Console.WriteLine("Cannot log to project '{0}' as it does not exist. Activate a different project.", _tpConfig.ActiveProject);
            } else if (message.StatusCode == HttpStatusCode.OK) {
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
                DateTime forDate = DateTime.Now.Date;
                if (args.Length == 4) {
                    DateTime forDateParsed;
                    if (DateTime.TryParseExact(args[3], "yyyy-M-d", null, DateTimeStyles.None, out forDateParsed)) {
                        forDate = forDateParsed;
                    } else {
                        Console.WriteLine("Incorrect date! Use [yyyy-M-d]");
                        return;
                    }
                }
                l.ForDate = forDate;

                string jsonContent = JsonConvert.SerializeObject(l);
                HttpResponseMessage response = await _client.PostAsync(_tpConfig.Origin + "/api/log", new StringContent(jsonContent, Encoding.UTF8, "application/json"));
                if (response.StatusCode == HttpStatusCode.OK) {
                    Console.WriteLine("Logged {0} hours to project {1}.", hours, p.Name);
                } else {
                    Console.WriteLine("Error: {0}", message.StatusCode.ToString());
                }
            } else {
                Console.WriteLine("Error: {0}", message.StatusCode.ToString());
            }
        }

        public async Task Show(string[] args) {
            if (args.Length > 2) {
                Console.WriteLine("Invalid amount of arguments. Use 'tp show', tp show [yyyy] or tp show [yyyy-M].");
                return;
            }
            string url = _tpConfig.Origin + "/api/log/" + WebUtility.UrlEncode(_tpConfig.ActiveProject);
            if (args.Length == 2) {
                string[] parts = args[1].Split('-');
                int year;
                int month;
                if (parts.Length > 2) {
                    Console.WriteLine("Invalid date. Use 'tp show', tp show [yyyy] or tp show [yyyy-M].");
                }
                if (parts.Length == 1) {
                    if (Int32.TryParse(parts[0], out year)) {
                        url += "/" + year.ToString();
                    } else {
                        Console.WriteLine("Invalid year!");
                    }
                }
                if (parts.Length == 2) {
                    if (Int32.TryParse(parts[0], out year) && Int32.TryParse(parts[1], out month) && month >= 1 && month <= 12) {
                        url += "/" + year.ToString() + "-" + month.ToString();
                    } else {
                        Console.WriteLine("Invalid date! Use 'tp show', tp show [yyyy] or tp show [yyyy-M].");
                    }
                }
            }
            HttpResponseMessage message = await _client.GetAsync(url);
            if (message.StatusCode == HttpStatusCode.OK) {
                string messageContent = await message.Content.ReadAsStringAsync();
                Log[] logs = JsonConvert.DeserializeObject<Log[]>(messageContent);
                Console.WriteLine("Log for project '{0}' in month '{1}':", _tpConfig.ActiveProject, DateTime.Now.ToString("yyyy-M"));
                Console.WriteLine();
                Console.WriteLine("{0, -10} {1}", "Date", "Hours");
                foreach (var item in logs) {
                    Console.WriteLine("{0, -10} {1}", item.ForDate.ToString("yyyy-M-d"), item.Hours);
                }
                Console.WriteLine("--------------------");
                Console.WriteLine("TOTAL      {0}", logs.Sum(i => i.Hours));
            } else if (message.StatusCode == HttpStatusCode.BadRequest) {
                Console.WriteLine("Cannot show log to project '{0}' as it does not exist. Activate a different project.", _tpConfig.ActiveProject);
            } else {
                Console.WriteLine("Error: {0}", message.StatusCode.ToString());
            }
        }

        public async Task ShowCustomers() {
            HttpResponseMessage message = await _client.GetAsync(_tpConfig.Origin + "/api/customer");
            string messageContent = await message.Content.ReadAsStringAsync();
            if (message.StatusCode == HttpStatusCode.OK) {
                Customer[] customers = JsonConvert.DeserializeObject<Customer[]>(messageContent);
                foreach (var item in customers) {
                    Console.WriteLine("{0, -5} {1}", item.Id, item.Name);
                }
            } else {
                Console.WriteLine("Error: {0}", message.StatusCode.ToString());
            }
        }

        public async Task ShowProjects() {
            HttpResponseMessage message = await _client.GetAsync(_tpConfig.Origin + "/api/project");
            string messageContent = await message.Content.ReadAsStringAsync();
            Project[] projects = JsonConvert.DeserializeObject<Project[]>(messageContent);
            foreach (var item in projects) {
                Console.WriteLine("{0, -5} {1, -20} {2}", item.Id, item.Name, item.Customer.Name);
            }
        }
    }
}