# UIAutomationCLI
Super lightweight Web Automation CLI on top of Selenium and Chromedriver

## Syntax
```
goto <url>
```
navigate to an url
```
find xpath <xpath>
find id <id>
find class <class>
find div <text>
find input <text>
find a <text>
find .. <text>
```
find/select an element on the page
```
test textcontains <text>
test textnotcontains <text>
test texteqs <text>
test enabled
```
test the found/selected element
```
sleep
```
sleep 10 seconds
```
waituntilenabled
```
Wait up to 10 seconds for selected element to enable
```
click
```
Click on selected element
```
type <text>
```
type to selected element
```
# comment
```
Output comment
```
end
```
Exit the tool

## Usage

```cmd
C:\> echo "goto google.com" | UIAutomationCLI.exe
```
```cmd
C:\> type testcase1.txt | UIAutomationCLI.exe > testresult.log
```