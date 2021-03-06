# Docker 部署

0. 安装 Docker 可以参阅 <https://yeasy.gitbook.io/docker_practice/install>
1. 找个文件夹;
2. 新建一个叫 `docker-compose.yml` 的文件, 将 <https://github.com/TRKS-Team/WFBot/blob/universal/docs/docker-compose.yml> 的内容复制到里面;
    > 直接执行 `wget https://ghproxy.com/https://raw.githubusercontent.com/TRKS-Team/WFBot/universal/docs/docker-compose.yml` 也可

3. 要**开启 WFBot**, 执行 `sudo docker-compose pull && sudo docker-compose run wfbot`
4. 所有配置文件均在 WFBotConfigs 文件夹下, 具体如何配置可以参阅 [**部署指南**](install.md)

---
也就是说, 在 Linux 下, 找个文件夹, 执行:
```bash
curl -fsSL get.docker.com -o get-docker.sh
sudo sh get-docker.sh --mirror Aliyun
wget https://ghproxy.com/https://raw.githubusercontent.com/TRKS-Team/WFBot/universal/docs/docker-compose.yml

sudo docker-compose pull && sudo docker-compose run wfbot
```

要**开启 WFBot**, 执行 `sudo docker-compose pull && sudo docker-compose run wfbot`  
如果你的用户在 docker 用户组内, 那就无需 sudo: `docker-compose pull && docker-compose run wfbot` 
