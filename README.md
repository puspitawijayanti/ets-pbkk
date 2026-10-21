# HunianKu

Apartment management desktop application built with C# WPF (.NET 10) and SQL Server. It covers towers and units, residents, lease contracts, monthly billing (rent, electricity, water, service charge), late fees, payments, maintenance requests, facility booking, and reports.

ETS project for the PBKK course.

| Member | Student ID | Responsibility |
|---|---|---|
| Puspita Wijayanti Kusuma | 5025241059 | Database, master data, complaints and booking, reports and dashboard |
| Nashwa Aulia Putri Diansyah | 5025241064 | Authentication, residents, lease contracts, role-based menu |
| Kineisha Rana Salsabila | 5025241149 | Project setup and theme, meter readings and billing, payments, publishing |

## Project Structure

```
ETS/
├── HunianKu.sln                 
├── database/
│   └── HunianKu.sql             
├── src/HunianKu/               
│   ├── App.xaml(.cs)            
│   ├── appsettings.json         
│   ├── Models/                  
│   ├── Data/                    
│   ├── Services/               
│   ├── Helpers/                 
│   ├── Views/                   
│   ├── Styles/Theme.xaml       
│   └── Properties/PublishProfiles/SingleFileExe.pubxml
├── publish/HunianKu/            
├── release/HunianKu-v1.0.zip    
└── docs/
    ├── PRD.md                                          
    ├── diagram/                 
    └── screenshots/             
```

## Requirements

To run the release build:
- Windows 10/11 (64-bit)
- SQL Server (Express, Developer, or LocalDB)

The executable is self-contained, so the .NET runtime does not need to be installed.

For development:
- Visual Studio 2026 with the .NET desktop development workload
- .NET SDK 10
- SQL Server and SQL Server Management Studio (SSMS)

## Database Setup

Run `database/HunianKu.sql` once, either by opening it in SSMS and executing it, or from the command line:

```bat
sqlcmd -S .\SQLEXPRESS -E -C -i database\HunianKu.sql
```

Replace `.\SQLEXPRESS` with your server name. The script drops and recreates the `HunianKu` database, including sample data, so it can also be used to reset the data.

## Configuration

The connection string is stored in `appsettings.json` (next to `HunianKu.exe`, or in `src/HunianKu/` during development):

```json
{
  "ConnectionStrings": {
    "HunianKu": "Server=.\SQLEXPRESS;Database=HunianKu;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Adjust the `Server` value to match your SQL Server instance:

| SQL Server instance | `Server` value |
|---|---|
| Express | `.\SQLEXPRESS` |
| Developer / Standard | `localhost` |
| LocalDB | `(localdb)\MSSQLLocalDB` |

To use SQL authentication, replace `Trusted_Connection=True` with `User Id=...;Password=...`.

## Running the Application

- Visual Studio: open `HunianKu.sln` and press F5.
- Command line: `dotnet run --project src/HunianKu`
- Release build: extract `release/HunianKu-v1.0.zip`, set up the database, adjust `appsettings.json`, then run `HunianKu.exe`.

### Sample Accounts

| Role | Username | Password |
|---|---|---|
| Admin | `admin` | `admin123` |
| Staff | `rina`, `dodi` | `staf123` |
| Resident | `budi`, `agus`, `siti`, `sarah` | `penghuni123` |

## Building the Executable

```bat
dotnet publish src/HunianKu/HunianKu.csproj -p:PublishProfile=SingleFileExe
```

The output is written to `publish/HunianKu/` and contains `HunianKu.exe` and `appsettings.json`.
