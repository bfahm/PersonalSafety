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

### Try these routes

- `localhost:[yourPortNumber]/api/Main/Index`
- `localhost:[yourPortNumber]/api/Main/TestRepository`

> This should return an Id and the word "test", the Id should increment at every page reload.

- `localhost:[yourPortNumber]/api/Main/TestRepositoryWithId/1`

> This should return an Id and the word "test", the Id should be the one provided in the requested URL.

- `localhost:[yourPortNumber]/api/Main/TestRepositoryWithId2?Id=1&Additional=2`

> This should return an Id, the Id should be the one provided in the requested URL, the parameter "Additional" is printed too.

- `localhost:[yourPortNumber]/api/Main/TestJson`

> This returns the full test object formatted in a JSON style.