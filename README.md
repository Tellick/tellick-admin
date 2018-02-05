# Tellick project tracking system
This is work in progress.

## Notes
Database is in memory for the time being.

## CLI usage

### connect to the system, the system will ask you to login
- [x] tp connect [url]

### add a new customer
- [x] tp new customer [customername]

### add a new project
- [x] tp new project [projectname] [customername]

### activate a project
- [x] tp active [projectname]

### log work for today (in hours), message is mandatory
- [x] tp log [number] [message]

### log work for a different date
- [x] tp log [number] [message] [yyyy-mm-dd]

### set the hourly rate
- [ ] tp set rate [money]

### show progress in this month
- [ ] tp show

### show progress in a month
- [ ] tp show [yyyy-mm]

### show progress in a year
- [ ] tp show [yyyy]

### generate invoices for this month
- [ ] tp invoice

### generate invoices for last month
- [ ] tp invoice lastmonth

### generate invoices for a month
- [ ] tp invoice -d [yyyy-mm]