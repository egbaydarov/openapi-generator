# Ockolo.CodeGen.OpenApiServerGenerator - A source generator for generating ASP.NET Core code from OpenAPI specifications.

## Usage

```js
<PropertyGroup>
    <ConfigOckoloCodeGenOpenApiServerGenerator>../generator-config.yaml</ConfigOckoloCodeGenOpenApiServerGenerator>
    <SpecOckoloCodeGenOpenApiServerGenerator>../open-api-spec.yaml</SpecOckoloCodeGenOpenApiServerGenerator>
</PropertyGroup>
<ItemGroup>
    <PackageReference Include="Ockolo.CodeGen.OpenApiServerGenerator" Version="<version>" PrivateAssets="all" />
    <PackageReference Include="Ockolo.CodeGen.OpenApiServerGenerator.Dependencies" Version="<version>" PrivateAssets="all" />
    <CompilerVisibleProperty Include="ConfigOckoloCodeGenOpenApiServerGenerator" />
    <CompilerVisibleProperty Include="SpecOckoloCodeGenOpenApiServerGenerator" />
</ItemGroup>
```
