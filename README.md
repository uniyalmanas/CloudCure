# CloudCure EMR & HMIS

CloudCure EMR is a modern, enterprise-grade, high-performance Hospital Management Information System (HMIS) and Electronic Medical Record (EMR) platform. Engineered for clinics, diagnostic centers, and multi-specialty hospitals, CloudCure has been modernized into a subscription-ready SaaS platform localized specifically for the Indian healthcare ecosystem.

---

## 🚀 Key Modernizations & Features

### 1. Modern .NET 8.0 & EF Core 8 Engine
* **High-Performance Runtime**: Completely upgraded from legacy .NET Framework configurations to the latest **.NET 8.0 (LTS)** runtime, running seamlessly on Kestrel.
* **Modern ORM (EF Core 8)**: Upgraded database mapping contexts to Entity Framework Core 8, optimizing query plan translation, eliminating TransactionScope deadlocks, and implementing explicit one-to-one schema mappings.
* **Central Session Middleware**: Restores stateless session integrity by parsing incoming Bearer JWT tokens and dynamically reconstructing user environments inside active requests, eliminating multi-module session timeouts.
* **System-Wide Command Protection**: Configured robust ADO.NET query command timeouts to guarantee SQL Server Express database reliability during intensive medical report generation.

### 2. Localization & Indian Market Customization
* **₹ INR Currency Standards**: Native mapping of Indian Rupee (`₹`) settings across all system configurations, receipts, accounting ledgers, and billing summaries.
* **Indian GST Taxes**: Configured centralized, compliant tax structures supporting automated CGST and SGST calculation rates on all medical consultation fees, pharmacy transactions, and ward packages.
* **SSU Form Simplification**: Cleaned up patient registration pipelines globally (SSU, Outpatient Billing, Emergency, and Government Insurance registration forms) by removing legacy Caste/Ethnic fields.

### 3. Integrated SaaS Dispatch & Billing Gateways
* **Dynamic UPI QR Code Checkout**: Instant scan-to-pay functionality built natively into the checkout panel. Patients can scan a dynamically generated UPI QR code on the spot using any standard app (GPay, PhonePe, Paytm, BHIM) to settle bills.
* **Scannable Print Invoicing**: Automated UPI payment QR code embedding directly at the bottom of printed invoices and digital receipts, allowing quick payments.
* **WhatsApp Report & Receipt Dispatch**: Seamless mock service hooks inside `DanpheEMR.Core` for automated dispatch of patient prescriptions, laboratory PDF reports, and billing receipts.

---

## 📦 Core Medical Modules

* **Patient Registration**: Optimized registration widgets pre-loading country subdivisions for district validation.
* **Appointment Scheduling**: Real-time slot management with department-wise doctor booking engines.
* **Billing & Invoicing**: Comprehensive insurance billing, copayments, dynamic UPI checkouts, and print layout engines.
* **Pharmacy & Inventory**: Complete batch tracking, counter activation queues, and prescription-to-dispensation workflows.
* **Clinical Cockpit & ADT**: Ward tracking, nursing worksheets, admission-discharge-transfer states, and doctor vitals boards.
* **Laboratory & Radiology**: HTML/normal report generators, sample collection barcodes, and final report approval workflows.

---

## 🛠️ How to Run Locally

### Prerequisites
* **Runtime**: [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* **Client-Side Build**: [Node.js (LTS)](https://nodejs.org)
* **Database**: SQL Server / LocalDB (Express or Developer edition)

### 1. Database Restoration
1. Restore the provided baseline SQL Server database backup located at:
   `Database/2. EMR-Db/DanpheInternationalDB/Dev_DanpheEMR_INT1.zip`
2. Update your SQL Server connection strings in the main configurations at:
   `Code/Websites/DanpheEMR/appsettings.json`

### 2. Run the Backend (Kestrel Server)
Navigate to the root directory and boot up the development server:
```powershell
dotnet run --project Code/Websites/DanpheEMR/DanpheEMR.csproj --launch-profile DanpheEMR
```
The server will start listening live on: **`http://localhost:5000`**

### 3. Build Client Assets
If you want to modify client assets, navigate to the Angular directory:
```bash
cd Code/Websites/DanpheEMR/wwwroot/DanpheApp
npm run build
```

---

## 🔒 Security & Compliance

* Centralized RBAC (Role-Based Access Control) managing permissions for doctors, cashiers, nurses, laboratory technicians, and administrators.
* Fully SSL-compliant endpoints supporting JWT Bearer authentication headers.
