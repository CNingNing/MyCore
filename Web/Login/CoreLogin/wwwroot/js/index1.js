$(function(){
	GetImgHtml();
})
function GetImgHtml()
{
	var _html="";
	for(var i=1;i<=50;i++)
	{
		var imgsrc="photo/photo%20("+i+").JPG";
		_html+="<img src='"+imgsrc+"'>";
	}
	$("#imgwrap").append(_html);
}