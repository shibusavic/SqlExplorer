# SqlExplorer

A utility for scanning Microsoft SQL Server databases and generating reports on structure and dependencies.

## /SqlExplorerCli

This project is the console application used to generate the reports.

### Usage

SqlExplorer {--connection-string | -c} <connection string> {--output-directory | -d} <directory>] [--overwrite | -o] [--help | -h | ?]

{--connection-string | -c} <connection string>  Define the connection string.
{--output-directory | -d} <directory>]          Define the output directory.
[--overwrite | -o]                              Overwrite output files if they exists.
[--help | -h | ?]                               Show this help.

Usages:

To generate reports for a given database:
        `SqlExplorer -d /c/temp/db -c "connection string"`

To ensure created files are overwritten:
        `SqlExplorer -d /c/temp/db -c "connection string" -o`

## /SqlServer

This project contains the business logic and database integration utilities.
