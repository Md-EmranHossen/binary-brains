# 🛒 AmarTech - An Ecommerce Platform

AmarTech is a robust E-Commerce Web Application built with ASP.NET Core MVC, showcasing key features of a modern online store. Created for educational purposes, it adheres to clean architecture principles, demonstrating how to develop a scalable and maintainable system from the ground up.

## 📚 Explore Project Resources, management

Dive into our [Wiki](https://github.com/Learnathon-By-Geeky-Solutions/binary-brains/wiki) for detailed documentation, follow progress with our [DevLog](https://github.com/Learnathon-By-Geeky-Solutions/binary-brains/tree/main/DevLog), and track tasks on our [Trello Board](https://trello.com/b/67a6303580ff372f899865ae/e-commerce-system-binary-brains).

<table align="center">
  <tr>
    <td>
      <a href="https://github.com/Learnathon-By-Geeky-Solutions/binary-brains/wiki">
        <img src="https://img.shields.io/badge/BinaryBrains-Wiki-007ACC?logo=github&logoColor=white&style=for-the-badge" alt="GitHub Wiki" />
      </a>
    </td>
    <td>
      <a href="https://trello.com/b/67a6303580ff372f899865ae/e-commerce-system-binary-brains">
        <img src="https://img.shields.io/badge/Trello-Project%20Board-0079BF?logo=trello&logoColor=white&style=for-the-badge" alt="Trello Board" />
      </a>
    </td>
    <td>
      <a href="https://github.com/Learnathon-By-Geeky-Solutions/binary-brains/tree/main/DevLog">
        <img src="https://img.shields.io/badge/DevLog-Updates-FFD700?logo=github&logoColor=white&style=for-the-badge" alt="DevLog" />
      </a>
    </td>
  </tr>
</table>

## Status and QualityDashboard

<div align="center">
  <table>
    <tr>
      <td><img src="https://sonarcloud.io/api/project_badges/measure?project=Learnathon-By-Geeky-Solutions_binary-brains&metric=alert_status&style=for-the-badge&color=4C8BF5" alt="Quality Gate" /></td>
      <td><img src="https://sonarcloud.io/api/project_badges/measure?project=Learnathon-By-Geeky-Solutions_binary-brains&metric=vulnerabilities&style=for-the-badge&color=FF6F61" alt="Vulnerabilities" /></td>
      <td><img src="https://sonarcloud.io/api/project_badges/measure?project=Learnathon-By-Geeky-Solutions_binary-brains&metric=bugs&style=for-the-badge&color=FF6F61" alt="Bugs" /></td>
      <td><img src="https://sonarcloud.io/api/project_badges/measure?project=Learnathon-By-Geeky-Solutions_binary-brains&metric=security_rating&style=for-the-badge&color=28A745" alt="Security" /></td>
      <td><img src="https://sonarcloud.io/api/project_badges/measure?project=Learnathon-By-Geeky-Solutions_binary-brains&metric=code_smells&style=for-the-badge&color=FFA500" alt="Code Smells" /></td>
    </tr>
   <tr>
      <td><img src="https://img.shields.io/github/contributors/Learnathon-By-Geeky-Solutions/binary-brains.svg?style=for-the-badge&color=4C8BF5" alt="Contributors" /></td>
      <td><img src="https://img.shields.io/github/forks/Learnathon-By-Geeky-Solutions/binary-brains.svg?style=for-the-badge&color=4C8BF5" alt="Forks" /></td>
      <td><img src="https://img.shields.io/github/stars/Learnathon-By-Geeky-Solutions/binary-brains.svg?style=for-the-badge&color=4C8BF5" alt="Stargazers" /></td>
      <td><img src="https://img.shields.io/github/issues/Learnathon-By-Geeky-Solutions/binary-brains.svg?style=for-the-badge&color=FF6F61" alt="Issues" /></td>
      <td><img src="https://img.shields.io/github/license/Learnathon-By-Geeky-Solutions/binary-brains.svg?style=for-the-badge&color=28A745" alt="License" /></td>
    </tr>

  </table>
</div>

## 🤝 Team Information: Binary Brains

<div align="center">

| 👤 Name     | Mashrief Bin Zulfiquer                       | Md Emran Hossen                                     | Md Rifatul                                     | FI Pranto                                     |
| ----------- | -------------------------------------------- | --------------------------------------------------- | ---------------------------------------------- | --------------------------------------------- |
| 🎯 Role     | Mentor                                       | Team Leader                                         | Member                                         | Member                                        |
| 💻 GitHub   | [mashrief](https://github.com/mashrief)      | [Md-EmranHossen](https://github.com/Md-EmranHossen) | [md-rifatul](https://github.com/md-rifatul)    | [FI-Pranto](https://github.com/FI-Pranto)     |
| 🔗 LinkedIn | [LinkedIn](https://linkedin.com/in/mashrief) | [LinkedIn](https://linkedin.com/in/md-emranhossen)  | [LinkedIn](https://linkedin.com/in/md-rifatul) | [LinkedIn](https://linkedin.com/in/fi-pranto) |

</div>

## 📅 Learning & Project Planning

### Stack Learning

➡️ **[Learning Phase Tracking Sheet](https://docs.google.com/spreadsheets/d/1O1THgzEOz3rn8fNiuz1fPZaR_eUYecXm_UKkXdEvVFY/edit?usp=sharing)** – Track our daily learning activities.

## 📝 Project Description

**AmarTech** is a full-featured e-commerce platform built using **ASP.NET Core MVC** following **Clean Architecture principles**. It is designed to offer both a learning resource for developers and a practical online shopping experience for users. AmarTech supports role-based access, secure transactions, product management, and much more, all presented through a responsive and intuitive UI.

## 🚀 Key Features

### 🛒 Product & Category Management

- Admins or employees can **add, edit, and delete products and categories**.
- Products include details like pricing, discounts, stock, and images.

### 👥 Role-Based Access Control

- Supports roles such as **Admin, Customer, Employee, and Company**.
- Each role has distinct access rights for managing different parts of the system.

### 🛍️ Shopping Cart & Checkout

- Users can **add items to a cart as a guest** (stored in memory) or as an authenticated user (stored in the database).
- At login, guest carts are merged with the user’s persistent cart.
- Includes **secure checkout** and **Stripe payment integration**.

### 🔒 Authentication & User Management

- Users can **register and log in using email/password**.
- Optional **Facebook login** integration.
- Admins can manage users and their roles from the dashboard.

### 📦 Order Tracking & History

- Customers can **track the status** of their orders and view order history.
- Admins can update order statuses and view order details.

---

## 🧠 Architectural Overview

### 🧱 Code Structure

- **Clean Architecture** with layers: Domain, Application, Infrastructure, Web, and Test.

### 🛠 Design Patterns

- Implements industry best practices such as:
  - **Dependency Injection**
  - **Repository Pattern**
  - **Service Layer Abstraction**
  - **Unit of Work** for database consistency

## 📁 Repository Structure

```
Src/
└── ECommerceSystem/
    ├── AmarTech.Application/
    │   ├── Contract/
    │   └── Services/
    │
    ├── AmarTech.Domain/
    │   └── Entities/
    │
    ├── AmarTech.Infrastructure/
    │   ├── Data/
    │   ├── DbInitializer/
    │   ├── Migrations/
    │   └── Repository/
    │
    ├── AmarTech.Web/
    │   ├── Areas/
    │   │   ├── Admin/
    │   │   │   ├── Controllers/
    │   │   │   └── Views/
    │   │   ├── Customer/
    │   │   │   ├── Controllers/
    │   │   │   └── Views/
    │   │   └── Identity/
    │   │       └── Pages/
    │   ├── Views/
    │   ├── wwwroot/
    │   └── Properties/
    │
    ├── AmarTech.Test/
    │   ├── ControllerTests/
    │   ├── RepositoryTests/
    │   └── ServiceTests/
    │
    └── ECommerceSystem.sln
```

---

## 📦 Resources

- [Project Documentation](docs/)
- [Development Setup Guide](docs/setup.md)
- [Contributing Guidelines](CONTRIBUTING.md)
