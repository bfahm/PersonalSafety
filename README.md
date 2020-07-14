# Personal Safety Backend

### Introduction
Simply described as a Location-based Emergency system, this platform gives you the ability to ask for help from both the **Authorities**, and **other users**.
You can send an **SOS requests** to the Police with a tap of a button. You can call for an **Ambulance to your current location** with a tap of a button, you can even ask for a Tow Truck in the same manner!
Users can also notify their peers about **locations to avoid**, and other events to **look out for**.

We got you covered when it comes to any form of help. It is Safety, made simple. All on your phone, in your pocket, wherever you go.

### Server Screenshots
![HomePage](https://lh4.googleusercontent.com/XjDjDAPRd2dQgyFNE0_bhjedyx0Y3ZVp8LXqEDJFk2CZBPDPEdBbh91BRfe1XTkj1feBCpj4EcYwcvNfY4vq=w1920-h842-rw)
![HomePage2](https://lh4.googleusercontent.com/4jdIymWq377WrchuZq9jgQroBBlafwdO2jqyjVj1MhrsrMgcnLdHjjiGZRPNHPm5A-C2ZX_XaTh4Fo-zXI3D=w1920-h842-rw)
![RealtimeClient for testing SignalR before implementing on mobile devices](https://lh5.googleusercontent.com/6avTYceXK1RmKotAkIpT1H35vZg8eztXdVS76gYjLfPvjgf_qyPJs4c_hzumqGog0r3XSrtJ1IIVJ1sZ7jqh=w1920-h842-rw)
![RealtimeClient up and running, connection ID shown and ready for copying, further, messages recieved from other participants are shown in the window too.](https://lh5.googleusercontent.com/CLeKgWaty9zKUUQ-cn2kDM--zfrL9MEEjJOdiyqW7-rgy6xrQtZCzlzI-kQrUUZJmDSfBFIYbDqBnAYFus3Q=w1920-h842-rw)
![Facebook authentication tool, retirieves access token for logged in user for testing purposes](https://lh3.googleusercontent.com/sAm-OakLrRAcb_WU-GYPvHnWgyCosuiIProG9Wxp_EyfCsXJSdc7jT16JcwRWntpkfAx3UqjR-u8N1D1qOSj=w1920-h842-rw)
![Swagger UI](https://lh4.googleusercontent.com/NPbGAXyGwBJcmtpHz9QuhAumwDCk5OxyJmP6kjoAWsDTbL3svrvLnWr46xVHOvvkPFeoQtBaHDPz3mbS26qk=w1920-h842-rw)
<img src="https://lh5.googleusercontent.com/2unatj1qiW672OEuGnvOVQsJJhXaaK6HzGlVn2zLKkZqfanVFRYo6Yf7IOKhf76XzDr5ha4TUAwPrQigxmB2=w1920-h842-rw" style="width:100%;">
![enter image description here](https://lh6.googleusercontent.com/-ztB1vN0Xi493AhyS8ZK989KZbX9Y_etfX4XH5f4vHdwkChArbC875-rQkp-Gqa8_FcnzP_kc2aed76gZzr_=w1920-h842-rw)
![enter image description here](https://lh5.googleusercontent.com/2o9e3GBci2ssuC4VJ_QOCkHG7JA65ZqVpx5f6yIQGst-7i65LIH7s0nE-XkJ5EEbnLrET9x4cvV0h-YoiJaZ=w1920-h842-rw)
![enter image description here](https://lh5.googleusercontent.com/u6Wvzz5Y8m4-ruxqaI6WayhvuDSZiKZVqIGFSoE_HhzVZtYX2WVA-RZa1UbulHGye0vuRm3I5PzQZOhMA54p=w1920-h842-rw)
![enter image description here](https://lh3.googleusercontent.com/-WDs4ds3YiypEHq9NKB4CZ5ck-KJZHUsTQNCdlKdblxsfNFJAnke-3bghp9GX450mq7rLhoEpjjfC1zmmn18=w1920-h842-rw)

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