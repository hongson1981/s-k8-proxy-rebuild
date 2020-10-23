# ICAP server - MinIO & RabbitMQ communication
## Description of the work

This is a code that submits a file from a local directory to MinIO and retrieve a MinIO URL and sends it as a message to the RabbitMQ queue with the MinIO URL path and then Download the URL from MiniIO and save the file in a local directory called "Incoming"


---

# Installation   
**.Net core Install**


 1.For Ubuntu 20.04 (LTS)
 
Open a terminal and run the following commands:
(Help url - https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#2004-):
 
 `` 
 wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
  ``
  
 Install the SDK :
 
 ``sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-3.1
 ``
 
For other Linux versions:
https://docs.microsoft.com/en-us/dotnet/core/install/linux


**RabbitMQ Install**
For Ubuntu 20.04
(Help url - https://www.rabbitmq.com/install-debian.html#manual-installation)

1.Run below commands to get Erlang as that is necessarry for RabbitMQ installation:
``
sudo tee /etc/apt/sources.list.d/bintray.rabbitmq.list <<EOF
deb https://dl.bintray.com/rabbitmq-erlang/debian bionic erlang
deb https://dl.bintray.com/rabbitmq/debian bionic main
EOF
``

2.Install RabbitMQ Package
First run update:
``
sudo apt-get update -y
``
Then install the package with:
``
sudo apt-get install -y rabbitmq-server
``
Now start the RabbitMQ service:
``
sudo service rabbitmq-server start
``

 

---

To test the code, do this
1. Clone the repository
``git clone https://github.com/k8-proxy/s-k8-proxy-rebuild.git
``
go inside the folder
``
cd s-k8-proxy-rebuild
``
then checkout to develop
``
git checkout develop
``
ASSUMING THE CODE IS MERGED TO DEV
``
cd upwork-devs/mohit-sengar
``
1. First start receiver
go inside MinIORabbitMQReceiver folder 
``
cd MinIORabbitMQReceiver
``
and run below command:
``
dotnet run
``
3. Then start sender for this first start new terminal
and go inside MinIORabbitMQSender folder 
``
cd MinIORabbitMQSender
``
and run below command:
``
dotnet run
``

**Code should be running at this point now and you will see all the console messages from sender and receiver terminal along with a new file created inside incoming folder under MinIORabbitMQReceiver**

    