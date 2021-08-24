#!/bin/sh

# 运行 xvfb
Xvfb -ac -screen scrn 1280x2000x24 :9.0 &
#echo "updateing"
# 更新
#wget -O /app/resources/app.asar https://github.com/fuck-xuexiqiangguo/Fuck-XueXiQiangGuo/raw/master/app.asar

# 运行程序
echo "starting"
dotnet xuexihelper.dll