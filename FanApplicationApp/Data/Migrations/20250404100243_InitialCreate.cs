using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpeedCalc.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aerodynamics_data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gas_type = table.Column<string>(type: "text", nullable: false),
                    aerodynamic_scheme = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    a1 = table.Column<double>(type: "numeric(18,6)", nullable: false),
                    a2 = table.Column<double>(type: "numeric(18,6)", nullable: false),
                    a3 = table.Column<double>(type: "numeric(18,6)", nullable: false),
                    b1 = table.Column<double>(type: "numeric(18,6)", nullable: false),
                    b2 = table.Column<double>(type: "numeric(18,6)", nullable: false),
                    b3 = table.Column<double>(type: "numeric(18,6)", nullable: false),
                    b4 = table.Column<double>(type: "numeric(18,6)", nullable: false),
                    min_speed = table.Column<double>(type: "numeric(18,2)", nullable: false),
                    max_speed = table.Column<double>(type: "numeric(18,2)", nullable: false),
                    a = table.Column<double>(type: "numeric(18,3)", nullable: false),
                    b = table.Column<double>(type: "numeric(18,3)", nullable: false),
                    min_delta = table.Column<double>(type: "numeric(18,4)", nullable: false),
                    max_delta = table.Column<double>(type: "numeric(18,4)", nullable: false),
                    new_mark_of_fan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    blade_length = table.Column<double>(type: "numeric(18,6)", nullable: false),
                    average_blade_width = table.Column<double>(type: "numeric(18,6)", nullable: false),
                    impeller_width = table.Column<double>(type: "numeric(18,3)", nullable: false),
                    impeller_inlet_diameter = table.Column<double>(type: "numeric(18,3)", nullable: false),
                    number_of_blades = table.Column<int>(type: "integer", nullable: false),
                    type_of_blades = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    new_mark_of_fan_d = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aerodynamics_data", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aerodynamics_data");
        }
    }
}
