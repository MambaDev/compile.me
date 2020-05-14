#!/bin/bash

########################################################################
#	- This is the main script that is used to compile/interpret the source code
#	- The script takes 3 arguments
#		1. The compiler that is to compile the source file.
#		2. The source file that is to be compiled/interpreted
#		3. Additional argument only needed for compilers, to execute the object code
#
#	- Sample execution command:   $: ./script.sh g++ file.cpp ./a.out
#
########################################################################

compiler=$1
sourceFile=$2
stdInFile=$3
output=$4
additionalArguments=$5
standard_out=$6
standard_error_out=$7

########################################################################
#	- The script works as follows
#	- It first stores the stdout and std err to another stream
#	- The output of the stream is then sent to respective files
#
#
#	- if third argument is empty Branch 1 is followed. An interpreter was called
#	- else Branch2 is followed, a compiler was invoked
#	- In Branch2. We first check if the compile operation was a success (code returned 0)
#
#	- If the return code from compile is 0 follow Branch2a and call the output command
#	- Else follow Branch2b and output error Message
#
#	- Stderr and Stdout are restored
#	- Once the logfile is completely written, it is renamed to "completed"
#	- The purpose of creating the "completed" file is because NodeJs searches for this file
#	- Upon finding this file, the NodeJS Api returns its content to the browser and deletes the folder
#
#
########################################################################

exec 1>"${standard_out}"
exec 2>"${standard_error_out}"
#3>&1 4>&2 >

START=$(date +%s.%2N)

if [ "$output" = "" ]; then
  $compiler "$sourceFile" - <"${stdInFile}" #| tee /usercode/output.txt
else
  #In case of compile errors, redirect them to a file
  $compiler "$sourceFile" "$additionalArguments" #&> /usercode/errors.txt
  if [ $? -eq 0 ]; then
    $output - <"${stdInFile}" #| tee /usercode/output.txt
  else
    echo "Compilation Failed"
  fi
fi

#exec 1>&3 2>&4

END=$(date +%s.%2N)
runtime=$(echo "$END - $START" | bc)

echo "*-COMPILE::EOF-*" "$runtime"
