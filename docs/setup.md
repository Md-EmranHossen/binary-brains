
# Getting Started

Follow these steps to set up and run the AmarTech E-Commerce System locally:

### 1. Clone the Repository
First, clone the repository to your local machine using the following command:
```bash
git clone https://github.com/Learnathon-By-Geeky-Solutions/binary-brains.git
cd binary-brains
```

### 2. Open the Solution in Visual Studio
- Open the `ECommerceApp.sln` solution file with **Visual Studio 2022**.
- Let Visual Studio restore all the NuGet packages.

### 3. Set Up the Database
This project uses **Entity Framework Core** with the **Code-First** approach.

- Open the `appsettings.json` file and configure the connection string as follows:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
Replace `YOUR_SERVER_NAME` with the name of your database server.

- Open the **Package Manager Console** in Visual Studio and run the following commands to apply the migrations and set up the database:

```bash
Add-Migration InitialCreate
Update-Database
```

This will create the database schema for the project.

### 4. Configure Authentication (Optional - Facebook Login)
If you want to enable **Facebook Login** authentication, you need to add the following code in the `Program.cs` file:

```csharp
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "your-facebook-app-id";
    options.AppSecret = "your-facebook-app-secret";
});
```
Make sure to replace `"your-facebook-app-id"` and `"your-facebook-app-secret"` with your actual Facebook app credentials.

### 5. Run the Application
- After configuring the database and authentication (if needed), run the application using the following command in Visual Studio:
```bash
dotnet run
```

The application will be available at `https://localhost:5001` (or the port specified in the output).

### 6. Configure Stripe (Optional)
If you plan to use **Stripe** for payments:
- Sign up for a Stripe account at [https://stripe.com/](https://stripe.com).
- Get your **Publishable Key** and **Secret Key** from your Stripe account.
- Add these keys to the `appsettings.json` under the `Stripe` section:
```json
"Stripe": {
  "PublishableKey": "your-publishable-key",
  "SecretKey": "your-secret-key"
}
```

### 7. Access the Application
Once the application is running, open your browser and go to:
```
https://localhost:5001
```
### 8. Tools & Technologies Used

- **ASP.NET Core MVC**
- **ASP.NET Core Identity**
- **Entity Framework Core**
- **SQL Server**
- **Bootstrap & JavaScript**
- **Stripe API (for payments)**
- **xUnit & Moq (for testing)**
- **Facebook OAuth**

You can start using the application by registering as a user or logging in as an admin to manage products and orders.

