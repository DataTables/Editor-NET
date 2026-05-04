# DataTables .NET server-side libraries

This is a collection of .NET libraries (.NET Framework, .NET Core and .NET) to provide easy server-side support for [DataTables](https://datatables.net/) - _the_ Javascript table library.

These libraries provide support for:

* Server-side processing - work with millions of rows
* Editor - CRUD UI for DataTables
* ColumnControl - Column search controls for DataTables
* SearchBuilder - Complex search logic UI

The library is framework-agnostic and can be used in any web framework.


## Installation

Available on [NuGet](https://www.nuget.org/packages/datatables-editor-server), this package can be installed with:

```sh
dotnet add package DataTables-Editor-Server
```

The library introduces the `DataTables` namespace, under which you will find the relevent classes and methods. There are two primary entry points:

* `DataTable` - for read only tables
* `Editor` - for read / write tables, with [Editor](https://editor.datatables.net/)


## Quick Start

A database connection is required for these libraries to operate. SQLServer, Postgres, MySQL and SQLite are all supported. To create a new connection you can use the `Database` class passing in the database type and a connection string, or if you already have an ADO connection, pass that into the `Database` constructor.

The following shows a WebAPI endpoint that will accept both DataTables and Editor requests. note that fields can be defined using an existing class by passing it through to `Model<T>` as a generic, or fields can be defined with additional options through the `Field` class:

```csharp
using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using DataTables;

namespace EditorNetDemo.Controllers
{
    public class StaffController : Controller
    {
        [Route("api/staff")]
        [HttpGet]
        [HttpPost]
        public ActionResult Staff()
        {
            var dbType = Environment.GetEnvironmentVariable("DBTYPE");
            var dbConnection = Environment.GetEnvironmentVariable("DBCONNECTION");

            using (var db = new Database(dbType, dbConnection))
            {
                var response = new Editor(db, "datatables_demo")
                    .Model<StaffModel>()
                    .Field(new Field("extn")
                        .Validator(Validation.Numeric())
                    )
                    .Field(new Field("age")
                        .Validator(Validation.Numeric())
                        .SetFormatter(Format.IfEmpty(null))
                    )
                    .Field(new Field("salary")
                        .Validator(Validation.Numeric())
                        .SetFormatter(Format.IfEmpty(null))
                    )
                    .Field(new Field("start_date")
                        .Validator(Validation.DateFormat(
                            Format.DATE_ISO_8601,
                            new ValidationOpts { Message = "Please enter a date in the format yyyy-mm-dd" }
                        ))
                        .GetFormatter(Format.DateSqlToFormat(Format.DATE_ISO_8601))
                        .SetFormatter(Format.DateFormatToSql(Format.DATE_ISO_8601))
                    )
                    .Process(Request)
                    .Data();

                return Json(response);
            }
        }
    }
}
```

Similarly, if your table is readonly, the `DataTable` and `Column` classes can be used:

```csharp
using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using DataTables;

namespace EditorNetDemo.Controllers
{
    public class StaffController : Controller
    {
        [Route("api/staff")]
        [HttpGet]
        [HttpPost]
        public ActionResult Staff()
        {
            var dbType = Environment.GetEnvironmentVariable("DBTYPE");
            var dbConnection = Environment.GetEnvironmentVariable("DBCONNECTION");

            using (var db = new Database(dbType, dbConnection))
            {
                var response = new Editor(db, "datatables_demo")
                    .Column(new Column("first_name") )
                    .Column(new Column("last_name"))
                    .Column(new Column("extn"))
                    .Column(new Column("age"))
                    .Column(new Column("salary"))
                    .Column(new Column("start_date"))
                    .Process(Request)
                    .Data();

                return Json(response);
            }
        }
    }
}
```

### Documentation

For full documentation, [please refer to the DataTables site](https://datatables.net/manual/net).


## License

MIT — see [LICENSE](LICENSE) for full text.
