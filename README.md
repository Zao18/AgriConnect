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


Reference list

Patil, J. (2022). Implementation of Unit Test using Xunit and Moq in .NET Core 6 Web API. [online] Medium. Available at: https://medium.com/@jaydeepvpatil225/implementation-of-unit-test-using-xunit-and-moq-in-net-core-6-web-api-539205f1d38f.

Renan (2021). What Are Alternative Farming Systems? [online] Tractor Transport. Available at: https://www.tractortransport.com/blog/what-are-alternative-farming-systems/.

Rick-Anderson (2024a). Role-based authorization in ASP.NET Core. [online] Microsoft.com. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-9.0.

Rick-Anderson (2024b). Use cookie authentication without ASP.NET Core Identity. [online] Microsoft.com. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0.

tdykstra (2024). Tutorial: Add sorting, filtering, and paging - ASP.NET MVC with EF Core. [online] Microsoft.com. Available at: https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page?view=aspnetcore-9.0&utm_source=chatgpt.com [Accessed 14 May 2025].

W3Schools (1999). W3Schools online web tutorials. [online] W3schools.com. Available at: https://www.w3schools.com/.

www.youtube.com. (n.d.). SHA-256 | COMPLETE Step-By-Step Explanation (W/ Example). [online] Available at: https://www.youtube.com/watch?v=orIgy2MjqrA [Accessed 6 Mar. 2024].
