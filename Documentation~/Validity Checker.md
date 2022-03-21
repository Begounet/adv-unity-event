# Validity Checker

## Overview

The validity checker is a tool allowing to analyze the whole project and give information about validity of AUE things.

Supports:

- coherency between the parameter mode (Constant, Dynamic etc.) and the real instanced custom argument type (AUECAConstant, AUECADynamic etc.)

## Usage

Save your scene before starting the tool (because scenes will be loaded and unloaded during the analysis).

Start from the validity checker from `Tools/AdvUnityEvent/Check Validity`.

The background task manager will give you information about what is going on, and the console will display the errors that could occurs during the check. If anything is written, then everything is okay.