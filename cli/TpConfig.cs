using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace tellick_admin.Cli {

    [DataContract]
    public class TpConfig {
        [DataMember]
        public string Origin { get; set; }
        [DataMember]
        public string Bearer { get; set; }
        [DataMember]
        public string ActiveProject { get; set; }        
    }
    
    public class TpConfigReaderWriter {
        private string _tpConfigPath;

        public TpConfig TpConfig { get; set; }

        public TpConfigReaderWriter() {
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string tpPath = Path.Combine(userProfilePath, ".tp");
            _tpConfigPath = Path.Combine(tpPath, ".tpconfig");

            if (Directory.Exists(tpPath) == false) {
                Directory.CreateDirectory(tpPath);
            }
            if (File.Exists(_tpConfigPath)) {
                TpConfig config = JsonConvert.DeserializeObject<TpConfig>(File.ReadAllText(_tpConfigPath));
                this.TpConfig = config;
            } else {
                this.TpConfig = new TpConfig();
                string json = JsonConvert.SerializeObject(this.TpConfig);
                File.WriteAllText(_tpConfigPath, json);
            }
        }

        public async Task WriteConfig() {
            string json = JsonConvert.SerializeObject(this.TpConfig);
            await File.WriteAllTextAsync(_tpConfigPath, json);
        }
    }
}