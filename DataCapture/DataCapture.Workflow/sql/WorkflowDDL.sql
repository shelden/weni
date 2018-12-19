# Commands for creating the Workflow Database Tables in a MySQL database.
# The order of these statements is important, and takes into consideration the creation of foreign keys.
# Also, MySQL database table names are case sensitive. For easier typing, we use all lower-case table names.

# This is case sensitive.
CREATE DATABASE IF NOT EXISTS workflow;
USE workflow;

###################################################################
# Drop the tables in reverse order for FK purposes:
DROP TABLE IF EXISTS audit_trail;
DROP TABLE IF EXISTS work_item_access;
DROP TABLE IF EXISTS work_item_data;
DROP TABLE IF EXISTS work_items;
DROP TABLE IF EXISTS rules;
DROP TABLE IF EXISTS steps;
DROP TABLE IF EXISTS maps;
DROP TABLE IF EXISTS allowed_queues;
DROP TABLE IF EXISTS queues;
DROP TABLE IF EXISTS sessions;
DROP TABLE IF EXISTS users;

##################################################################
# Create the Users table.
CREATE TABLE IF NOT EXISTS users
(
     USER_ID        INT(10)         UNSIGNED NOT NULL AUTO_INCREMENT,
     LOGIN          VARCHAR(32)     NOT NULL,
     LOGIN_LIMIT    SMALLINT(5)     UNSIGNED NOT NULL DEFAULT 1,
     STATUS         SMALLINT(5)     UNSIGNED NOT NULL DEFAULT 1,

     PRIMARY KEY (USER_ID),
     UNIQUE KEY (LOGIN),
     INDEX   user_login_idx (LOGIN)
 );

##################################################################
# Create the Sessions table, which references the Users table.
CREATE TABLE IF NOT EXISTS sessions
(
     SESSION_ID     INT(10)         UNSIGNED NOT NULL AUTO_INCREMENT,
     USER_ID        INT(10)         UNSIGNED NOT NULL,
     HOST_NAME      VARCHAR(32)     NOT NULL,
     START_TIME     DATETIME(6)     NOT NULL,

     PRIMARY KEY (SESSION_ID),
     FOREIGN KEY sessions_user_fk (USER_ID)
        REFERENCES users (USER_ID)
        ON DELETE RESTRICT
);

##################################################################
# Create the Queues table
CREATE TABLE IF NOT EXISTS queues
(
     QUEUE_ID       INT(10)         UNSIGNED NOT NULL AUTO_INCREMENT,
     NAME           VARCHAR(32)     NOT NULL,
     TYPE           SMALLINT(5)     UNSIGNED NOT NULL DEFAULT 1,

     PRIMARY KEY (QUEUE_ID),
     UNIQUE KEY (NAME),
     INDEX queue_name_idx (NAME)
);

##################################################################
# Create the Allowed Queues table, which references the Users and the Queues tables.
CREATE TABLE IF NOT EXISTS allowed_queues
(
     ALLOWED_ID     INT(10)         UNSIGNED NOT NULL AUTO_INCREMENT,
     USER_ID        INT(10)         UNSIGNED NOT NULL,
     QUEUE_ID       INT(10)         UNSIGNED NOT NULL,

     PRIMARY KEY (ALLOWED_ID),
     FOREIGN KEY allowed_user_fk (USER_ID)
        REFERENCES users (USER_ID)
        ON DELETE RESTRICT,
     FOREIGN KEY allowed_queue_fk (QUEUE_ID)
        REFERENCES queues (QUEUE_ID)
        ON DELETE CASCADE
);

##################################################################
# Create the Maps table
CREATE TABLE IF NOT EXISTS maps
(
     MAP_ID         INT(10)         UNSIGNED NOT NULL AUTO_INCREMENT,
     NAME           VARCHAR(32)     NOT NULL,
     VERSION        SMALLINT(5)     UNSIGNED NOT NULL DEFAULT 1,

     PRIMARY KEY (MAP_ID),
     UNIQUE KEY (NAME)
);

##################################################################
# Create the Steps table, which references the Maps and the Queues tables.
CREATE TABLE IF NOT EXISTS steps
(
     STEP_ID        INT(10)         UNSIGNED NOT NULL AUTO_INCREMENT,
     MAP_ID         INT(10)         UNSIGNED NOT NULL,
     QUEUE_ID       INT(10)         UNSIGNED NOT NULL,
     NAME           VARCHAR(32)     NOT NULL,
     NEXT_STEP_ID   INT(10)         UNSIGNED,
     TYPE           SMALLINT(5)     NOT NULL,

     PRIMARY KEY (STEP_ID),
     FOREIGN KEY steps_map_fk (MAP_ID)
        REFERENCES maps (MAP_ID)
        ON DELETE RESTRICT,
     FOREIGN KEY steps_queue_fk (QUEUE_ID)
        REFERENCES queues (QUEUE_ID)
        ON DELETE RESTRICT,
     FOREIGN KEY steps_next_fk (NEXT_STEP_ID)
        REFERENCES steps (STEP_ID)
        ON DELETE SET NULL
);

##################################################################
# Create the Rules table, which references the Steps table.
CREATE TABLE IF NOT EXISTS rules
(
     RULE_ID        INT(10)         UNSIGNED NOT NULL AUTO_INCREMENT,
     STEP_ID        INT(10)         UNSIGNED NOT NULL,
     RULE_ORDER     INT(10)         UNSIGNED NOT NULL DEFAULT 1,
     VARIABLE_NAME  VARCHAR(32)     NOT NULL,
     VARIABLE_VALUE VARCHAR(256)    NOT NULL,
     COMPARISON     INT(10)         UNSIGNED NOT NULL DEFAULT 1,
     NEXT_STEP_ID   INT(10)         UNSIGNED NOT NULL,

     PRIMARY KEY (RULE_ID),
     FOREIGN KEY rules_step_fk (STEP_ID)
        REFERENCES steps (STEP_ID)
        ON DELETE CASCADE,
     FOREIGN KEY rules_next_fk (NEXT_STEP_ID)
        REFERENCES steps (STEP_ID)
        ON DELETE CASCADE
);

##################################################################
# Create the Work Items table, which references the Steps table.
CREATE TABLE IF NOT EXISTS work_items
(
     ITEM_ID        INT(10)         UNSIGNED NOT NULL AUTO_INCREMENT,
     STEP_ID        INT(10)         UNSIGNED NOT NULL,
     NAME           VARCHAR(32)     DEFAULT NULL,
     STATE          SMALLINT(5)     NOT NULL DEFAULT 1,
     PRIORITY       INT(10)         NOT NULL DEFAULT 0,
     CREATED        DATETIME(6)     NOT NULL,
     ENTERED        DATETIME(6)     NOT NULL,
     SESSION_ID     INT(10)         UNSIGNED,

     PRIMARY KEY (ITEM_ID),
     FOREIGN KEY items_step_fk (STEP_ID)
        REFERENCES steps (STEP_ID)
        ON DELETE RESTRICT,
     FOREIGN KEY items_session_fk (SESSION_ID)
        REFERENCES sessions (SESSION_ID)
        ON DELETE SET NULL
 );

##################################################################
# Create the Work Item Data table, which references the Work Item table.
CREATE TABLE IF NOT EXISTS work_item_data
(
     DATA_ID        INT(10)         UNSIGNED NOT NULL AUTO_INCREMENT,
     ITEM_ID        INT(10)         UNSIGNED NOT NULL,
     VARIABLE_NAME  VARCHAR(32)     NOT NULL,
     VARIABLE_VALUE VARCHAR(256)    NOT NULL,

     PRIMARY KEY (DATA_ID),
     FOREIGN KEY data_item_fk (ITEM_ID)
        REFERENCES work_items (ITEM_ID)
        ON DELETE CASCADE
);

##################################################################
# Create the Work Item Access table, which references the Work Item and User tables.
CREATE TABLE IF NOT EXISTS work_item_access
(
     ACCESS_ID      INT(10)         UNSIGNED NOT NULL AUTO_INCREMENT,
     ITEM_ID        INT(10)         UNSIGNED NOT NULL,
     USER_ID        INT(10)         UNSIGNED NOT NULL,
     PERMISSION     SMALLINT(5)     NOT NULL,

     PRIMARY KEY (ACCESS_ID),
     FOREIGN KEY access_item_fk (ITEM_ID)
        REFERENCES work_items (ITEM_ID)
        ON DELETE CASCADE,
     FOREIGN KEY access_user_fk (USER_ID)
        REFERENCES users (USER_ID)
        ON DELETE CASCADE
 );

##################################################################
# Create the Work Item Access table, which references the Work Item and User tables.
CREATE TABLE IF NOT EXISTS audit_trail
(
     AUDIT_ID       BIGINT(12)      UNSIGNED NOT NULL AUTO_INCREMENT,
     ITEM_ID        INT(10)         UNSIGNED NOT NULL,
     USER_ID        INT(10)         UNSIGNED,
     STEP_ID        INT(10)         UNSIGNED NOT NULL,
     AUDIT_DTIME    DATETIME(6)     NOT NULL,
     EVENT          SMALLINT(5)     NOT NULL,

     PRIMARY KEY (AUDIT_ID),
     FOREIGN KEY audit_item_fk (ITEM_ID)
        REFERENCES work_items (ITEM_ID)
        ON DELETE CASCADE,
     FOREIGN KEY audit_user_fk (USER_ID)
        REFERENCES users (USER_ID)
        ON DELETE SET NULL,
     FOREIGN KEY audit_step_fk (STEP_ID)
        REFERENCES steps (STEP_ID)
        ON DELETE RESTRICT
);
