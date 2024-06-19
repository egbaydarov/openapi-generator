java -cp ".:../../../modules/openapi-generator-cli/target/classes:../../../modules/openapi-generator-cli/target/lib/*" org.openapitools.codegen.OpenAPIGenerator generate \
 -g aspnetcore \
 -i ./test-config.yml \
 -o ./ \
 -c ./generator-config.yml