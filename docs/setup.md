# 🚀 Getting Started

## 1. Clone the Repository

```bash
git clone https://github.com/Learnathon-By-Geeky-Solutions/binary-brains.git
cd binary-brains
```

## 2. Open the Solution in Visual Studio

- Open `ECommerceApp.sln` with **Visual Studio 2022**
- Let Visual Studio restore all NuGet packages

## 3. Set Up the Database

> This project uses **Entity Framework Core** with **Code-First** approach.

### Configure `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Apply Migrations:

Open **Package Manager Console**:

```powershell
Update-Database
```

> This will create and seed the database.

---

# ▶️ Running the Application

1. Set `ECommerceApp` as the startup project.
2. Press `F5` or click the **Run** button.
3. The site will open in your default browser at `https://localhost:xxxx/`
