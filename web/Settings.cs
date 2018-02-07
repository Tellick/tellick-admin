using System;

namespace tellick_admin {
    public static class Settings {
        public static string ConnectionString {
            get {
                #if DEBUG
                return "Server=.;Database=tellick-admin;Trusted_Connection=True;";
                #else
                return Environment.GetEnvironmentVariable("TELLICK-ADMIN-ConnectionString");
                #endif
            }
        }

        public static string JwtIssuer {
            get {
                #if DEBUG
                return "http://localhost:5000";
                #else
                return Environment.GetEnvironmentVariable("TELLICK-ADMIN-JwtIssuer");
                #endif
            }
        }

        public static string JwtAudience {
            get {
                #if DEBUG
                return "http://localhost:5000";
                #else
                return Environment.GetEnvironmentVariable("TELLICK-ADMIN-JwtAudience");
                #endif
            }
        }

        public static string JwtSigningKey {
            get {
                #if DEBUG
                return "thisismydevelopmentkey";
                #else
                return Environment.GetEnvironmentVariable("TELLICK-ADMIN-JwtAudience");
                #endif
            }
        }
    }
}