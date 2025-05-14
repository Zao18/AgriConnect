AgriConnect

## 📦 Development Environment Setup

1. Install Prerequisites:
   - [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
   - [Visual Studio 2022+](https://visualstudio.microsoft.com/) or Visual Studio Code
   - Optional: [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) for local Azure Storage emulation

2. Clone the Project:
   ```bash
   git clone https://github.com/yourusername/AgriConnect.git
   cd AgriConnect

3. Configure Azure Table Storage:

Open appsettings.json:

Add your Azure Storage connection string under a key such as "StorageConnectionString"

"StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=youraccount;AccountKey=yourkey;EndpointSuffix=core.windows.net"


▶️ How to Build and Run the Prototype
Open the solution in Visual Studio (AgriConnect.sln).

Build the project using Build > Build Solution.

Run the project by pressing F5 or selecting Debug > Start Debugging.


🌐 System Functionalities
Users can log in with a username and password.

Credentials are checked against Azure Table Storage.

Login fails if the credentials are incorrect and displays an appropriate error.

After successful login, users are redirected based on their role.


👥 User Roles
Admin

- Access to administrative features and dashboards.

General User

- Access to standard user interface.

Farmer

- Access to farmer-specific views and data.
