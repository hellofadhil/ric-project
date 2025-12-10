# Running Backend:

cd OnePro.API
dotnet run --urls "https://localhost:58644"

# Running Frontend

cd OnePro.Front
dotnet run --urls "https://localhost:7095"

# Migrations dan Update Database

cd Core
dotnet ef migrations add AddNameTable --startup-project ../OnePro.API
dotnet ef database update --startup-project ../OnePro.API

# Open Sql Server in CMD

sqlcmd -S (localdb)\MSSQLLocalDB -Q "DROP DATABASE DatabaseRic"

sqlcmd -S (localdb)\MSSQLLocalDB -d DatabaseRic -Q "SELECT TOP 20 * FROM FormRics"
