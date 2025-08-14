#!/bin/bash
xsd=`which xsd`
xjc=`which xjc`
if [ -x "$xsd" ]
then
  mkdir -p dotnet
  echo Building C# code...
  xsd cicm.xsd /classes /language:CS > /dev/null
  if [ -e "cicm.cs" ]
  then
    mv cicm.cs dotnet/
  fi
  echo Building VB.NET code...
  xsd cicm.xsd /classes /language:VB > /dev/null
  if [ -e "cicm.vb" ]
  then
    mv cicm.vb dotnet/
  fi
fi

if [ -x "$xjc" ]
then
  mkdir -p java
  echo Building Java code...
  xjc -d java cicm.xsd > /dev/null
fi