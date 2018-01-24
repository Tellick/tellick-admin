## connect to the system, the system will ask you to login
tp connect [url]

## add a new customer
tp new customer [customername]

## add a new project
tp new project [projectname] [customername]

## activate a project
tp active [projectname]

## log work for today (in hours), message is mandatory
tp log [number] [message]

## log work for a different date
tp log [number] [message] [yyyy-mm-dd]

## set the hourly rate
tp set rate [money]

## show progress in this month
tp show

## show progress in a month
tp show -d [yyyy-mm]

## show progress in a year
tp show -d [yyyy]

## generate invoices for this month
tp invoice

## generate invoices for last month
tp invoice lastmonth

## generate invoices for a month
tp invoice -d [yyyy-mm]