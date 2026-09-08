CREATE DATABASE IF NOT EXISTS fitora_user;
CREATE DATABASE IF NOT EXISTS fitora_interact;
CREATE DATABASE IF NOT EXISTS fitora_notification;
GRANT ALL PRIVILEGES ON fitora_user.* TO 'fitora'@'%';
GRANT ALL PRIVILEGES ON fitora_interact.* TO 'fitora'@'%';
GRANT ALL PRIVILEGES ON fitora_notification.* TO 'fitora'@'%';
FLUSH PRIVILEGES;
