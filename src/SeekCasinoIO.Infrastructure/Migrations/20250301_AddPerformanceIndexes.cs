using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeekCasinoIO.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Performans için ekstra indeksler ekliyoruz
            
            // 1. Casino tablosunda arama alanları için indeksler
            migrationBuilder.CreateIndex(
                name: "IX_Casinos_Name",
                table: "Casinos",
                column: "Name");
                
            migrationBuilder.CreateIndex(
                name: "IX_Casinos_Point",
                table: "Casinos",
                column: "Point");
            
            // 2. Ara tablolar için composite indeksler (many-to-many ilişkiler)
            migrationBuilder.CreateIndex(
                name: "IX_CasinoGameType_GameTypesId_CasinosId",
                table: "CasinoGameType",
                columns: new[] { "GameTypesId", "CasinosId" });
                
            migrationBuilder.CreateIndex(
                name: "IX_CasinoLanguage_LanguagesId_CasinosId",
                table: "CasinoLanguage",
                columns: new[] { "LanguagesId", "CasinosId" });
                
            migrationBuilder.CreateIndex(
                name: "IX_CasinoLicence_LicencesId_CasinosId",
                table: "CasinoLicence",
                columns: new[] { "LicencesId", "CasinosId" });
                
            migrationBuilder.CreateIndex(
                name: "IX_CasinoPaymentMethod_PaymentMethodsId_CasinosId",
                table: "CasinoPaymentMethod",
                columns: new[] { "PaymentMethodsId", "CasinosId" });
                
            migrationBuilder.CreateIndex(
                name: "IX_CasinoProvider_ProvidersId_CasinosId",
                table: "CasinoProvider",
                columns: new[] { "ProvidersId", "CasinosId" });
            
            // 3. İlişkili tablolar için indeksler
            migrationBuilder.CreateIndex(
                name: "IX_GameTypes_Name",
                table: "GameTypes",
                column: "Name");
                
            migrationBuilder.CreateIndex(
                name: "IX_Languages_Name",
                table: "Languages",
                column: "Name");
                
            migrationBuilder.CreateIndex(
                name: "IX_Licences_Name",
                table: "Licences",
                column: "Name");
                
            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Name",
                table: "PaymentMethods",
                column: "Name");
                
            migrationBuilder.CreateIndex(
                name: "IX_Providers_Name",
                table: "Providers",
                column: "Name");
                
            // 4. Tarih alanları için indeksler (sıralama, filtreleme için)
            migrationBuilder.CreateIndex(
                name: "IX_Casinos_AddedAtUtc",
                table: "Casinos",
                column: "AddedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // İndeksleri kaldır
            migrationBuilder.DropIndex(
                name: "IX_Casinos_Name",
                table: "Casinos");
                
            migrationBuilder.DropIndex(
                name: "IX_Casinos_Point",
                table: "Casinos");
                
            migrationBuilder.DropIndex(
                name: "IX_CasinoGameType_GameTypesId_CasinosId",
                table: "CasinoGameType");
                
            migrationBuilder.DropIndex(
                name: "IX_CasinoLanguage_LanguagesId_CasinosId",
                table: "CasinoLanguage");
                
            migrationBuilder.DropIndex(
                name: "IX_CasinoLicence_LicencesId_CasinosId",
                table: "CasinoLicence");
                
            migrationBuilder.DropIndex(
                name: "IX_CasinoPaymentMethod_PaymentMethodsId_CasinosId",
                table: "CasinoPaymentMethod");
                
            migrationBuilder.DropIndex(
                name: "IX_CasinoProvider_ProvidersId_CasinosId",
                table: "CasinoProvider");
                
            migrationBuilder.DropIndex(
                name: "IX_GameTypes_Name",
                table: "GameTypes");
                
            migrationBuilder.DropIndex(
                name: "IX_Languages_Name",
                table: "Languages");
                
            migrationBuilder.DropIndex(
                name: "IX_Licences_Name",
                table: "Licences");
                
            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_Name",
                table: "PaymentMethods");
                
            migrationBuilder.DropIndex(
                name: "IX_Providers_Name",
                table: "Providers");
                
            migrationBuilder.DropIndex(
                name: "IX_Casinos_AddedAtUtc",
                table: "Casinos");
        }
    }
}