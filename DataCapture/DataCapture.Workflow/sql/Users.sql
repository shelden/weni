create user if not exists 'shelden'@'localhost';
set password for 'shelden'@'localhost' = '';
grant all on workflow.* to 'shelden'@'localhost';
create user if not exists 'workflow'@'localhost';
set password for 'workflow'@'localhost' = '';
grant all on workflow.* to 'workflow'@'localhost';
