// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data.Properties;

namespace Microsoft.Practices.EnterpriseLibrary.Data
{
    /// <summary>
    /// ConnectionString class constructs a connection string by 
    /// inserting a username and password into a template.
    /// </summary>
    public class ConnectionString
    {
        private const char CONNSTRING_DELIM = ';';
        private string connectionString;
        private string connectionStringWithoutCredentials;
        private string userIdTokens;
        private string passwordTokens;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionString"/> with a connection string, the user ID tokens and password tokens.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="userIdTokens">The user id tokens that can be parsed out of the connection string.</param>
        /// <param name="passwordTokens">The password tokens that can be parsed out of the connection string.</param>
        /// <exception cref="ArgumentException">One of the parameters is <b>null</b> or empty.</exception>
        public ConnectionString(string connectionString, string userIdTokens, string passwordTokens)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, nameof(connectionString));
            if (string.IsNullOrEmpty(userIdTokens)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, nameof(userIdTokens));
            if (string.IsNullOrEmpty(passwordTokens)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, nameof(passwordTokens));

            this.connectionString = connectionString;
            this.userIdTokens = userIdTokens;
            this.passwordTokens = passwordTokens;

            this.connectionStringWithoutCredentials = null;
        }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// Database username for the connection string.
        /// </value>
        public string UserName
        {
            get
            {
                string lowConnString = connectionString.ToLowerInvariant();
                int uidPos;
                int uidMPos;

                GetTokenPositions(userIdTokens, out uidPos, out uidMPos);
                if (0 <= uidPos)
                {
                    // found a user id, so pull out the value
                    int uidEPos = lowConnString.IndexOf(CONNSTRING_DELIM, uidMPos);
                    return connectionString.Substring(uidMPos, uidEPos - uidMPos);
                }
                else
                {
                    return String.Empty;
                }
            }
            set
            {
                string lowConnString = connectionString.ToLowerInvariant();
                int uidPos;
                int uidMPos;
                GetTokenPositions(userIdTokens, out uidPos, out uidMPos);
                if (0 <= uidPos)
                {
                    // found a user id, so replace the value
                    int uidEPos = lowConnString.IndexOf(CONNSTRING_DELIM, uidMPos);
                    connectionString = connectionString.Substring(0, uidMPos) +
                        value + connectionString.Substring(uidEPos);

                    //_connectionStringNoCredentials = RemoveCredentials(_connectionString);
                }
                else
                {
                    //no user id in the connection string so just append to the connection string
                    string[] tokens = userIdTokens.Split(',');
                    connectionString += tokens[0] + value + CONNSTRING_DELIM;
                }
            }
        }

        /// <summary>
        /// Gets or sets the user password for the connection string.
        /// </summary>
        public string Password
        {
            get
            {

                string lowConnString = connectionString.ToLowerInvariant();
                int pwdPos;
                int pwdMPos;
                GetTokenPositions(passwordTokens, out pwdPos, out pwdMPos);

                if (0 <= pwdPos)
                {
                    // found a password, so pull out the value
                    int pwdEPos = lowConnString.IndexOf(CONNSTRING_DELIM, pwdMPos);
                    return connectionString.Substring(pwdMPos, pwdEPos - pwdMPos);
                }
                else
                {
                    return String.Empty;
                }
            }
            set
            {
                string lowConnString = connectionString.ToLowerInvariant();
                int pwdPos;
                int pwdMPos;
                GetTokenPositions(passwordTokens, out pwdPos, out pwdMPos);

                if (0 <= pwdPos)
                {
                    // found a password, so replace the value
                    int pwdEPos = lowConnString.IndexOf(CONNSTRING_DELIM, pwdMPos);
                    connectionString = connectionString.Substring(0, pwdMPos) + value + connectionString.Substring(pwdEPos);

                    //_connectionStringNoCredentials = RemoveCredentials(_connectionString);
                }
                else
                {
                    //no password in the connection string so just append to the connection string
                    string[] tokens = passwordTokens.Split(',');
                    connectionString += tokens[0] + value + CONNSTRING_DELIM;
                }
            }
        }

        /// <summary>
        /// Gets the formatted connection string.
        /// </summary>        
        public override string ToString()
        {
            return connectionString;
        }

        /// <summary>
        /// Gets the formatted connection string without the username and password.
        /// </summary>        
        public string ToStringNoCredentials()
        {
            if (connectionStringWithoutCredentials == null)
                connectionStringWithoutCredentials = RemoveCredentials(connectionString);
            return connectionStringWithoutCredentials;
        }

        /// <summary>
        /// Formats a new connection string with a user ID and password.
        /// </summary>  
        /// <param name="connectionStringToFormat">
        /// The connection string to format.
        /// </param>        
        public ConnectionString CreateNewConnectionString(string connectionStringToFormat)
        {
            return new ConnectionString(connectionStringToFormat, userIdTokens, passwordTokens);
        }

        private void GetTokenPositions(string tokenString, out int tokenPos, out int tokenMPos)
        {
            string[] tokens = tokenString.Split(',');
            int previousPos = -1;
            string lowConnString = connectionString.ToLowerInvariant();

            //initialize output parameter
            tokenPos = -1;
            tokenMPos = -1;
            foreach (string token in tokens)
            {
                int currentPos = lowConnString.IndexOf(token, StringComparison.OrdinalIgnoreCase);
                if (currentPos > previousPos)
                {
                    tokenPos = currentPos;
                    tokenMPos = currentPos + token.Length;
                    previousPos = currentPos;
                }
            }
        }

        private string RemoveCredentials(string connectionStringToModify)
        {
            StringBuilder connStringNoCredentials = new StringBuilder();

            string[] tokens = connectionStringToModify.ToLowerInvariant().Split(CONNSTRING_DELIM);

            string thingsToRemove = userIdTokens + "," + passwordTokens;
            string[] avoidTokens = thingsToRemove.ToLowerInvariant().Split(',');

            foreach (string section in tokens)
            {
                bool found = false;
                string token = section.Trim();
                if (token.Length != 0)
                {
                    foreach (string avoidToken in avoidTokens)
                    {
                        if (token.StartsWith(avoidToken, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        connStringNoCredentials.Append(token + CONNSTRING_DELIM);
                    }
                }
            }
            return connStringNoCredentials.ToString();
        }
    }
}
