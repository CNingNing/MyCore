$(function() {
	$(".navigation").find("a").click(function() {
		var url = $(this).attr("href");
		if(url != "#") {
			var str = "<iframe src='" + url + "' width='100%' height='100%'></iframe>"
			$(".content .row").append($(str));
			return false;
		}

	})

});