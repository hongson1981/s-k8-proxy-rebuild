## Requirements
1. Ubuntu LTS VM (tested on 18.04)
2. Python3 virtualenv (on the build machine)


## Preparing environment
1. On the target machine, create non-root user on the target ubuntu system

```
sudo useradd -m user
```

2. Allow the user to execute sudo without password

```
echo 'user ALL=(ALL) NOPASSWD: ALL' | sudo tee /etc/sudoers.d/user
```

3. On your computer, switch to the directory containing this README guide and create a virtual environment after cloning the repo

```
python3 -m virtualenv --python=python3 venv
```

## Usage

1. Activate the virtual environment

```
source venv/bin/activate
```

2. Set the target vm ip and execute init.sh, for example

```
IP=192.168.56.10 ./init.sh
```

3. Create SSH key pairs, 

```
ssh-keygen
```

4. Transfer the SSH public key to the target VM, for example

```
ssh-copy-id -i ~/.ssh/id_rsa.pub user@192.168.56.10
```

6. Run the playbook

```
ansible-playbook --private-key ~/.ssh/id_rsa -i cluster/hosts.yaml --become --become-user=root --flush-cache -u user kubespray/cluster.yml
```
