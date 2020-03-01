# Personal Safety Backend

### Setup Guide
This guide assumes that all required NuGET packages are already installed.

1. Make sure SQL Server is set up and ready for new databases
2. Copy the `ServerName` that appear when trying to connect the SQLServer at its launch
3. In `appsettings.json` file paster the `ServerName` that you copied in step 2 so it replaces `DESKTOP-MHDRF11`.
4. Migrations needed to build up the database are already done (check the folder name `Migrations` in the root project directory)
5. Launch Package Manager Console from `Tools > NuGET Package Manager > Package Manager Console`
6. Run the command `Update-Database`
7. Check SQLServer to see if a database name "PersonalSafety" was added.
8. Clean, Rebuild, then Run
9. If error occurs, make sure to install all needed NuGET packages first and then try again

### NuGET Packages Dependencies

- `Microsoft.AspNetCore.Authentication.JwtBearer` - Version **3.1.2**
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`- Version **3.1.2**
- `Microsoft.AspNetCore.Identity.UI`- Version **3.1.2**
- `Microsoft.EntityFrameworkCore.SqlServer`- Version **3.1.1**
- `Microsoft.EntityFrameworkCore.Tools`- Version **3.1.1**
- `Microsoft.VisualStudio.Web.CodeGeneration.Design`- Version **3.1.1**
- `NLog.Web.AspNetCore` - Version **4.9.0**
- `Swashbuckle.AspNetCore` - Version **5.0.0**