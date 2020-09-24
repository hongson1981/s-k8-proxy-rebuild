## Requirements
1. Ubuntu LTS VM (tested on 18.04)
2. Python3 virtualenv (on the build machine)


## Usage
1. On the target machine, create non-root user on the target ubuntu system

```
useradd -m user
```

2. Allow the user to execute sudo without password

```
echo 'user ALL=(ALL) NOPASSWD: ALL' > /etc/sudoers.d/user
```

3. On your computer, switch to the directory containing this README guide and create a virtual environment after cloning the repo
```
python3 -m virtualenv --python=python3 venv
. venv/bin/activate
```

4. set the target vm ip and execute init.sh , for example
```
IP=192.168.56.10 ./init.sh
```
