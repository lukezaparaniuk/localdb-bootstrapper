# SQL Server Express LocalDB Bootstrapper
A .NET library providing programmatic setup and teardown of SQL Server Express LocalDB instances and deployment of Visual Studio database projects. LocalDB is a lightweight, simplified version of SQL Server that requires minimal configuration yet offers the same features as Express. It is currently bundled with Visual Studio. Database projects allow you to define your schema as files within folders which are then published to a SQL Server instance. The intended use case of this library is integration testing where a clean database instance is required for each test iteration.

## Usage
The `LocalDbService` service class is used to create a LocalDB instance. It requires two constructor parameters: the path to `SqlLocalDB.exe`, which is the LocalDB executable and should live at `C:\Program Files\Microsoft SQL Server\120\Tools\Binn\`; the path to where LocalDB instances are stored on disk, which is usually `C:\Users\{user}\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\`.

To make an instance, call the `LocalDbService.Make` method, passing in a name.

To build and deploy a database project to the instance, call the `LocalDbService.BuildAndPublishProject` method, passing in the path to the `.sqlproj' file and desired database name.

Your instance can then be accessed using the `(localdb)\{instance}` server.

## What it does
Internally it handles the rigmarole of database instance management by performing the following actions:

1. If the instance already exists, find out what databases it has and delete them both from the instance and on disk, then stop and delete the instance
2. Delete the instance directory so we can create it anew, which requires that the LocalDB `sqlservr.exe` process is killed. This *shouldn't* misbehave, but do be aware that it happens
3. Creates a clean instance with a clean directory

## License
This project is licensed under the MIT [License](LICENSE).
