
# Personal Safety Backend

### Introduction
Simply described as a Location-based Emergency system, this platform gives you the ability to ask for help from both the **Authorities**, and **other users**.
You can send an **SOS requests** to the Police with a tap of a button. You can call for an **Ambulance to your current location** with a tap of a button, you can even ask for a Tow Truck in the same manner!
Users can also notify their peers about **locations to avoid**, and other events to **look out for**.

We got you covered when it comes to any form of help. It is Safety, made simple. All on your phone, in your pocket, wherever you go.

### Server Screenshots
![HomePage](https://i.imgur.com/lCTHLFl.png)
![HomePage2](https://i.imgur.com/Et4REDP.png)
![RealtimeClient for testing SignalR before implementing on mobile devices](https://i.imgur.com/7qUAZDo.png)
![RealtimeClient up and running, connection ID shown and ready for copying, further, messages recieved from other participants are shown in the window too.](https://i.imgur.com/KItHEIT.png)
![Facebook authentication tool, retirieves access token for logged in user for testing purposes](https://i.imgur.com/FkVAL3r.png)
![Swagger UI](https://i.imgur.com/WK2ZJLX.png)
<img src="https://i.imgur.com/7QrXldO.png" style="width:100%;">

![Realtime Console accessible through the admin panel](https://i.imgur.com/5182m38.png)
![Manually send push notifications to any device, or turn of the master switch](https://i.imgur.com/R8WZuzM.png)
![Email Confirmation](https://i.imgur.com/aTIrQZc.png)
![Password Recovery](https://i.imgur.com/Fduex34.png)

### Setup Guide
This guide assumes that all required NuGET packages are already installed. **If you are not sure, run a Clean and Rebuild before starting.**

1. Make sure SQL Server is set up and ready for new databases instances
2. Copy the `ServerName` that appear when trying to connect the SQLServer at its launch
3. In `appsettings.json` file paste the `ServerName` that you copied in step 2 so it replaces `DESKTOP-MHDRF11`.
4. Migrations needed to build up the database are already there (check the folder name `Migrations` in the root project directory), launch the Package Manager Console from `Tools > NuGET Package Manager > Package Manager Console`
5. Run the command `Update-Database`
6. Check SQLServer to see if a database name "PersonalSafety" was added.
7. Make sure the `Migration History` table was built to, if not, be sure to manually build it to avoid future migration problems.
8. Clean, Rebuild, then Run