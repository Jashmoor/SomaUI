let inside = false;
let timeout = null;

export function CreateChart(ref, instance) {
    if (!ref)
        return { width: 0 , height: 0 };


    ref.mouseMoveHandler = Throttle(function ({ clientX, clientY }) {
        if (inside) {
            var { left, top, ...ignore } = ref.getBoundingClientRect();
            try { instance.invokeMethodAsync('MouseMove', clientX - left, clientY - top); }
            catch { }
        }
    }, 200);

    ref.mouseEnterHandler = function () {
        console.log('mouse entered');
        inside = true;
    };

    ref.mouseLeaveHandler = function (e) {
        if (e.relatedTarget && (e.relatedTarget.matches('.rz-chart-tooltip') || e.relatedTarget.closest('.rz-chart-tooltip'))) {
            return;
        }
        console.log('mouse left');
        inside = false;
        try { instance.invokeMethodAsync('MouseMove', -1, -1); }
        catch { }
    };

    ref.clickHandler = function (e) {
        console.log('click handler');
        const { clientX, clientY } = e;
        var { left, top, ...ignore } = ref.getBoundingClientRect();
        if (!e.target.closest('.rz-marker') && !e.target.closest('.rz-legend-item')) {
            try { instance.invokeMethodAsync('Click', clientX - left, clientY - top); }
            catch { }
        }
    };

    ref.addEventListener('mouseenter', ref.mouseEnterHandler);
    ref.addEventListener('mouseleave', ref.mouseLeaveHandler);
    ref.addEventListener('mousemove', ref.mouseMoveHandler);
    ref.addEventListener('click', ref.clickHandler);

    return CreateResizable(ref, instance);
}

function Throttle(callback, delay) {
    timeout = null;
    return function () {
        var args = arguments;
        var ctx = this;
        if (!timeout) {
            timeout = setTimeout(function () {
                callback.apply(ctx, args);
                timeout = null;
            }, delay);
        }
    };
}

function CreateResizable(ref, instance) {
    ref.resizeHandler = function () {
        let { width, height, ...ignore } = ref.getBoundingClientRect();
        try { instance.invokeMethodAsync('Resize', width, height); }
        catch { }
    };

    // if (window.ResizeObserver) {
    //     ref.resizeObserver = new ResizeObserver(ref.resizeHandler);
    //     ref.resizeObserver.observe(ref);
    // } else {
    // }
        window.addEventListener('resize', ref.resizeHandler);

    const {
        width,
        height,
        ...ignore
    } = ref.getBoundingClientRect();

    return { width, height };
}