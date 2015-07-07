using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using BookPortal.Logging.Domain;

namespace BookPortalLoggingMigrations
{
    [ContextType(typeof(LogsContext))]
    partial class LogsContextModelSnapshot : ModelSnapshot
    {
        public override void BuildModel(ModelBuilder builder)
        {
            builder
                .Annotation("ProductVersion", "7.0.0-beta6-13675")
                .Annotation("SqlServer:ItentityStrategy", "IdentityColumn");

            builder.Entity("BookPortal.Logging.Domain.Log", b =>
                {
                    b.Property<Guid>("OperationContext");

                    b.Property<string>("Exception");

                    b.Property<string>("Layer");

                    b.Property<string>("Message");

                    b.Property<string>("Severity");

                    b.Property<DateTime>("Timestamp");

                    b.Key("OperationContext");

                    b.Annotation("Relational:TableName", "logs");
                });
        }
    }
}