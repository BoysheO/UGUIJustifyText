# UGUIJustifyText
UGUIJustifyText 两端对齐Text
支持多行两端对齐

*非等宽字体效果看上去会比较怪异，但是对于亚洲字体就会非常友好
*Non-monospace fonts can look weird, but they are very friendly to Asian fonts

支持夹杂空格的字符串

为什么非等宽字体看上去会比较怪异？
因为获取不到空白字符对应vertex，如果按字距算法算出来，就会缺了这个空白字符的空间。空白字符会用的很多，重新排列文字位置更加实惠。