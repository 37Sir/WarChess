#!/bin/sh
cd /usr/app/demoproxy/log
touch mt.out
chown -R kpgame:kpgame mt.out 

cd /usr/app/demoproxy/
chmod +x startup
./startup
