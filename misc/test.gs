
*label1
[add 1 1]:$va
[pop ebx]
[goto *label3]

* label2
[add ret 2]
[goto *end]

*start # start is a special label
# start here
- Hello World
- GalaScript
+ Test
[add 2 2]
[push ebx]
[goto *label1]

* label3
[add $va 2]
[goif *label2]

*end
