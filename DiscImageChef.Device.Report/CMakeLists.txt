cmake_minimum_required(VERSION 3.9)
project(DiscImageChef_Device_Report C)

set(CMAKE_C_STANDARD 90)

add_executable(DiscImageChef_Device_Report main.c scsi.c scsi.h main.h ata.h ata.c atapi.c atapi.h)