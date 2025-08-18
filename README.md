# Digital Arena Project

**Digital Arena** is a modern ASP.NET MVC 5 web application for buying, selling, and exploring digital products — from **interactive 3D models** to **e-books**, **graphics templates**, and **presentation slides**.

---

## 🚀 Key Highlights

-   **Real-time 3D Model Viewer** — Render and explore 3D assets directly in the browser with **Three.js**.
-   **Powerful Search** — Full-text product search powered by **Typesense** (Docker-based).
-   **E-Book Previews** — Partial PDF viewing without downloads.
-   **Trending Algorithm** — Weighted scoring to showcase popular products.

---

## 🌐 Live Demo & Credentials

You can access the live project here: **[http://digitalarena.somee.com/](http://digitalarena.somee.com/)**

Use the following credentials to explore different user roles:

-   **Admin Access:**
    -   **Username:** `ADMIN`
    -   **Password:** `Digital1#`
-   **Seller/Buyer Access:**
    -   **Username:** `azhar`
    -   **Password:** `Digital1#`

---

## ⚠️ Important Note on Demo Content

Due to free-tier hosting and limited storage, only a select number of products are available for full preview functionality. You can test the core features with the following items:

-   **Interactive 3D Models:**
    -   Shoe 3D Model
-   **E-Book Previews:**
    -   Atomic Habits
    -   Getting Motivated to Change

---

## 📌 Core Features

-   User authentication & profile management
-   Product browsing with categories & filters
-   Seller mode with product uploads & management
-   Shopping cart & wishlist
-   Payment simulation & order processing
-   Notification system for user updates
-   Responsive, mobile-friendly UI
-   Robust backend with **ASP.NET MVC** & **Entity Framework (Database-First)**

---

## 🛠️ Technologies

-   **ASP.NET MVC 5** & **Web API**
-   **C#**
-   **Entity Framework (Database-First)**
-   **MySQL / SQL Server / Oracle 10g**
-   **Docker** + **Typesense** (search server)
-   **Three.js** (3D model rendering)
-   **HTML, CSS, JavaScript, Bootstrap**
-   **GitHub** (version control)

---

## ⚡ Getting Started

### Prerequisites

-   Visual Studio 2019+
-   .NET Framework 4.7.2+
-   MySQL / SQL Server / Oracle database setup
-   Docker (for Typesense search)
-   Git

### Installation

1.  Clone the repository:
    ```bash
    git clone [https://github.com/AzharSabbir/DigitalArena-Project-ASP.Net.git](https://github.com/AzharSabbir/DigitalArena-Project-ASP.Net.git)
    ```
2.  Configure your database connection string in `Web.config`.
3.  Set up and run the Typesense container using Docker.
4.  Build and run the application from Visual Studio.
