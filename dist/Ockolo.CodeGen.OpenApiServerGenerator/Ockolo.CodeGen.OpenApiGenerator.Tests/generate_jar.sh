java -jar ../../../modules/openapi-generator-cli/target/openapi-generator-cli.jar generate \
 -g aspnetcore \
 -i ./test-config.yml \
 -o ./ \
 -c ./generator-config.yml