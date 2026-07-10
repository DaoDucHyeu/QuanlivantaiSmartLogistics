using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLogistics.Migrations
{
    /// <inheritdoc />
    public partial class RenameManagerAndTaiXeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DieuHanhs_ManagerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiXes_DieuHanhs_ManagerId",
                table: "TaiXes");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_DieuHanhs_ManagerId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_TaiXes_MaTaiXe",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_DieuHanhs_ManagerId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_TaiXes_MaTaiXe",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Vehicles",
                newName: "TaiXeId");

            migrationBuilder.RenameColumn(
                name: "MaTaiXe",
                table: "Vehicles",
                newName: "DieuHanhId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_MaTaiXe",
                table: "Vehicles",
                newName: "IX_Vehicles_DieuHanhId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_ManagerId",
                table: "Vehicles",
                newName: "IX_Vehicles_TaiXeId");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Trips",
                newName: "DieuHanhId");

            migrationBuilder.RenameColumn(
                name: "MaTaiXe",
                table: "Trips",
                newName: "TaiXeId");

            migrationBuilder.RenameIndex(
                name: "IX_Trips_MaTaiXe",
                table: "Trips",
                newName: "IX_Trips_TaiXeId");

            migrationBuilder.RenameIndex(
                name: "IX_Trips_ManagerId",
                table: "Trips",
                newName: "IX_Trips_DieuHanhId");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "TaiXes",
                newName: "DieuHanhId");

            migrationBuilder.RenameIndex(
                name: "IX_TaiXes_ManagerId",
                table: "TaiXes",
                newName: "IX_TaiXes_DieuHanhId");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Orders",
                newName: "DieuHanhId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_ManagerId",
                table: "Orders",
                newName: "IX_Orders_DieuHanhId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DieuHanhs_DieuHanhId",
                table: "Orders",
                column: "DieuHanhId",
                principalTable: "DieuHanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiXes_DieuHanhs_DieuHanhId",
                table: "TaiXes",
                column: "DieuHanhId",
                principalTable: "DieuHanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_DieuHanhs_DieuHanhId",
                table: "Trips",
                column: "DieuHanhId",
                principalTable: "DieuHanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_TaiXes_TaiXeId",
                table: "Trips",
                column: "TaiXeId",
                principalTable: "TaiXes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_DieuHanhs_DieuHanhId",
                table: "Vehicles",
                column: "DieuHanhId",
                principalTable: "DieuHanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_TaiXes_TaiXeId",
                table: "Vehicles",
                column: "TaiXeId",
                principalTable: "TaiXes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DieuHanhs_DieuHanhId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiXes_DieuHanhs_DieuHanhId",
                table: "TaiXes");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_DieuHanhs_DieuHanhId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_TaiXes_TaiXeId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_DieuHanhs_DieuHanhId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_TaiXes_TaiXeId",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "TaiXeId",
                table: "Vehicles",
                newName: "ManagerId");

            migrationBuilder.RenameColumn(
                name: "DieuHanhId",
                table: "Vehicles",
                newName: "MaTaiXe");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_TaiXeId",
                table: "Vehicles",
                newName: "IX_Vehicles_ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_DieuHanhId",
                table: "Vehicles",
                newName: "IX_Vehicles_MaTaiXe");

            migrationBuilder.RenameColumn(
                name: "TaiXeId",
                table: "Trips",
                newName: "MaTaiXe");

            migrationBuilder.RenameColumn(
                name: "DieuHanhId",
                table: "Trips",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Trips_TaiXeId",
                table: "Trips",
                newName: "IX_Trips_MaTaiXe");

            migrationBuilder.RenameIndex(
                name: "IX_Trips_DieuHanhId",
                table: "Trips",
                newName: "IX_Trips_ManagerId");

            migrationBuilder.RenameColumn(
                name: "DieuHanhId",
                table: "TaiXes",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_TaiXes_DieuHanhId",
                table: "TaiXes",
                newName: "IX_TaiXes_ManagerId");

            migrationBuilder.RenameColumn(
                name: "DieuHanhId",
                table: "Orders",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_DieuHanhId",
                table: "Orders",
                newName: "IX_Orders_ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DieuHanhs_ManagerId",
                table: "Orders",
                column: "ManagerId",
                principalTable: "DieuHanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiXes_DieuHanhs_ManagerId",
                table: "TaiXes",
                column: "ManagerId",
                principalTable: "DieuHanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_DieuHanhs_ManagerId",
                table: "Trips",
                column: "ManagerId",
                principalTable: "DieuHanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_TaiXes_MaTaiXe",
                table: "Trips",
                column: "MaTaiXe",
                principalTable: "TaiXes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_DieuHanhs_ManagerId",
                table: "Vehicles",
                column: "ManagerId",
                principalTable: "DieuHanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_TaiXes_MaTaiXe",
                table: "Vehicles",
                column: "MaTaiXe",
                principalTable: "TaiXes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
