(function () {
  if (!window.EventSource) {
    return;
  }

  var source = new EventSource("/__dev/live-reload");

  source.addEventListener("reload", function () {
    window.location.reload();
  });
})();
