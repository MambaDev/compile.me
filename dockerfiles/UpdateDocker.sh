
# echo "Creating Docker Image"
# docker build -t 'virtual_machine' - < Dockerfile

echo "Creating Docker Image - Python"
docker build -t 'virtual_machine_python' - < DockerfilePython

echo "Retrieving Installed Docker Images"
docker images

