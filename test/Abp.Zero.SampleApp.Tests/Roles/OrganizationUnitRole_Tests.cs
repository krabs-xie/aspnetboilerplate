﻿using System.Linq;
using System.Threading.Tasks;
using Abp.IdentityFramework;
using Abp.Organizations;
using Abp.Zero.SampleApp.Roles;
using Shouldly;
using Xunit;

namespace Abp.Zero.SampleApp.Tests.Roles
{
    public class OrganizationUnitRole_Tests : SampleAppTestBase
    {
        private readonly RoleManager _roleManager;

        public OrganizationUnitRole_Tests()
        {
            var defaultTenant = GetDefaultTenant();
            AbpSession.TenantId = defaultTenant.Id;
            _roleManager = Resolve<RoleManager>();
        }

        [Fact]
        public async Task Test_AddToOrganizationUnitAsync()
        {
            //Arrange
            var ou2 = GetOu("OU2");
            var role = await CreateRole("role_1");

            //Act
            await _roleManager.AddToOrganizationUnitAsync(role, ou2, AbpSession.TenantId);

            //Assert
            (await _roleManager.IsInOrganizationUnitAsync(role, ou2)).ShouldBe(true);
            UsingDbContext(context => context.OrganizationUnitRoles.FirstOrDefault(ou => ou.RoleId == role.Id && ou.OrganizationUnitId == ou2.Id).ShouldNotBeNull());
        }

        [Fact]
        public async Task Test_RemoveFromOrganizationUnitAsync()
        {
            //Arrange
            var ou11 = GetOu("OU11");
            var role = await CreateRole("role_1");

            //Act
            await _roleManager.RemoveFromOrganizationUnitAsync(role, ou11);

            //Assert
            (await _roleManager.IsInOrganizationUnitAsync(role, ou11)).ShouldBe(false);
            UsingDbContext(context => context.OrganizationUnitRoles.FirstOrDefault(ou => ou.RoleId == role.Id && ou.OrganizationUnitId == ou11.Id).ShouldBeNull());
        }

        [Fact]
        public async Task Should_Remove_Role_From_Organization_When_Role_Is_Deleted()
        {
            //Arrange
            var role = await CreateRole("role_1");
            var ou11 = GetOu("OU11");

            await _roleManager.AddToOrganizationUnitAsync(role, ou11, AbpSession.TenantId);
            (await _roleManager.IsInOrganizationUnitAsync(role, ou11)).ShouldBe(true);

            //Act
            (await _roleManager.DeleteAsync(role)).CheckErrors();

            //Assert
            (await _roleManager.IsInOrganizationUnitAsync(role, ou11)).ShouldBe(false);
        }

        [Theory]
        [InlineData(new object[] { new string[0] })]
        [InlineData(new object[] { new[] { "OU12", "OU21" } })]
        [InlineData(new object[] { new[] { "OU11", "OU12", "OU2" } })]
        public async Task Test_SetOrganizationUnitsAsync(string[] organizationUnitNames)
        {
            //Arrange
            var role = await CreateRole("role_1");
            var organizationUnitIds = organizationUnitNames.Select(oun => GetOu(oun).Id).ToArray();

            //Act
            await _roleManager.SetOrganizationUnitsAsync(role, AbpSession.TenantId, organizationUnitIds);

            //Assert
            UsingDbContext(context =>
            {
                context.OrganizationUnitRoles
                    .Count(uou => uou.RoleId == role.Id && organizationUnitIds.Contains(uou.OrganizationUnitId))
                    .ShouldBe(organizationUnitIds.Length);
            });
        }

        private OrganizationUnit GetOu(string displayName)
        {
            var organizationUnit = UsingDbContext(context => context.OrganizationUnits.FirstOrDefault(ou => ou.DisplayName == displayName));
            organizationUnit.ShouldNotBeNull();

            return organizationUnit;
        }
    }
}