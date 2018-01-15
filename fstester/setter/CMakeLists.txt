cmake_minimum_required(VERSION 3.10)
project(setter C)

set(CMAKE_C_STANDARD 90)

add_executable(setter consts.h defs.h main.h dosos2.h os2_16.h os2_32.h main.c dos.c os2_16.c os2_32.c win32.c unix.c)