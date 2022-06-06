window.addEventListener("load", function (event) {
	const authordiv = document.getElementsByClassName("byline-div")[0];
	let childindex = 0;
	for (let i = 0; i < authordiv.childNodes.length; i++) {
		if ((authordiv.childNodes[i].nodeType == 3) && (authordiv.childNodes[i].textContent.trim().length > 0)) {
			childindex = i;
		}
	}
	
	let helpspan = document.createElement("span");
	helpspan.classList.add("help-link");
	let helplink = document.createElement("a");
	helplink.href = "mailto:puzzhunt@microsoft.com?subject=%5B" + 
		encodeURIComponent(document.getElementsByTagName("h1")[0].innerText.split("\n")[0].trim()) +
		"%5D&body=The%20more%20details%20you%20add%20here%2C%20the%20more%20helpful%20the%20responses%20you%27ll%20get%20will%20be%21%20%F0%9F%98%87%0D%0A%0D%0A";
	helplink.textContent = "click here";
	helpspan.append(" (", helplink, " to ask the author for help)");
	const child = authordiv.childNodes[childindex + 1];
	authordiv.insertBefore(helpspan, child);
});