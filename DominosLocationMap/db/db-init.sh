sleep 25s

mkdir /var/opt/mssql/ReplData/
chown mssql /var/opt/mssql/ReplData/
chgrp mssql /var/opt/mssql/ReplData/

echo "running set up script"
/opt/mssql-tools/bin/sqlcmd -S localhost -U sqldockerusername -P Besiktas1903 -d master -i db-init.sql